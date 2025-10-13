# Arquitetura Técnica Detalhada - InfluenciAI Platform

## 1. DECISÕES ARQUITETURAIS FUNDAMENTAIS

### 1.1 Estilo Arquitetural
**Microserviços com Event-Driven Architecture**
- **Justificativa**: Escalabilidade independente por domínio, resiliência, deploy independente
- **Padrão**: Domain-Driven Design (DDD) com Bounded Contexts
- **Comunicação**: Assíncrona via mensageria + Síncrona via gRPC/REST
- **Orquestração**: Saga pattern para fluxos complexos

### 1.2 Stack Tecnológico Principal

#### Backend Core
- **Runtime**: .NET 8 (C# 12)
- **Framework**: ASP.NET Core para APIs
- **Messaging**: Azure Service Bus / RabbitMQ
- **Cache**: Redis Stack (Cache + TimeSeries + Search)
- **Database**: PostgreSQL 15 (principal) + MongoDB (analytics)

#### Integrações Redes Sociais
- **Twitter/X**: Tweetinvi + Twitter API v2
- **Instagram**: Instagram Basic Display API + Graph API
- **LinkedIn**: LinkedIn API v2 + Share API
- **TikTok**: TikTok API v2 + Web Scraping fallback
- **YouTube**: YouTube Data API v3

#### Machine Learning & AI
- **ML Framework**: ML.NET para modelos locais
- **LLM**: Azure OpenAI Service (GPT-4)
- **Computer Vision**: Azure Cognitive Services
- **Time Series**: Azure Time Series Insights
- **Feature Store**: Feast

#### Frontend
- **Desktop**: WPF com .NET 8 + MVVM
- **Web Admin**: Blazor Server
- **Mobile**: .NET MAUI
- **Dashboard**: SignalR para real-time

#### Infrastructure
- **Cloud**: Azure (primary) / AWS (DR)
- **Containers**: Docker + Kubernetes (AKS)
- **CI/CD**: Azure DevOps + GitHub Actions
- **Monitoring**: Application Insights + Prometheus + Grafana
- **Secrets**: Azure Key Vault

### 1.3 Padrões de Design

#### Domain Patterns
- **Repository Pattern**: Abstração de persistência
- **Unit of Work**: Transações consistentes
- **Specification Pattern**: Queries complexas
- **Factory Pattern**: Criação de publishers/collectors

#### Application Patterns
- **CQRS**: Separação Commands/Queries
- **Mediator**: MediatR para handlers
- **Pipeline**: Para processamento de dados
- **Circuit Breaker**: Resiliência em integrações

#### Infrastructure Patterns
- **Outbox Pattern**: Garantia de entrega de eventos
- **Retry + Backoff**: Para APIs externas
- **Bulkhead**: Isolamento de recursos
- **Cache-Aside**: Estratégia de cache

## 2. ARQUITETURA DE MICROSERVIÇOS

### 2.1 Serviço: Social Publisher Service

#### Responsabilidades
- Publicar conteúdo em múltiplas redes
- Adaptar conteúdo por rede
- Gerenciar rate limits
- Retry logic e error handling

#### Estrutura Interna
```
SocialPublisher.Service/
├── Domain/
│   ├── Entities/
│   │   ├── Content.cs
│   │   ├── PublishRequest.cs
│   │   └── PublishResult.cs
│   ├── ValueObjects/
│   │   ├── NetworkType.cs
│   │   ├── MediaContent.cs
│   │   └── PublishOptions.cs
│   └── Interfaces/
│       ├── INetworkPublisher.cs
│       └── IContentAdapter.cs
├── Application/
│   ├── Commands/
│   │   ├── PublishContentCommand.cs
│   │   └── SchedulePublishCommand.cs
│   ├── Handlers/
│   │   ├── PublishContentHandler.cs
│   │   └── SchedulePublishHandler.cs
│   └── Services/
│       ├── PublisherOrchestrator.cs
│       └── RateLimitManager.cs
├── Infrastructure/
│   ├── Publishers/
│   │   ├── TwitterPublisher.cs
│   │   ├── InstagramPublisher.cs
│   │   ├── LinkedInPublisher.cs
│   │   └── TikTokPublisher.cs
│   ├── Adapters/
│   │   ├── TwitterContentAdapter.cs
│   │   └── InstagramContentAdapter.cs
│   └── Persistence/
│       └── PublishHistoryRepository.cs
└── API/
    ├── Controllers/
    │   └── PublishController.cs
    └── gRPC/
        └── PublisherService.cs
```

#### APIs Expostas
```csharp
// REST API
POST /api/v1/publish
POST /api/v1/publish/schedule
GET  /api/v1/publish/{id}/status
DELETE /api/v1/publish/{id}

// gRPC Service
service Publisher {
    rpc PublishContent(PublishRequest) returns (PublishResponse);
    rpc ScheduleContent(ScheduleRequest) returns (ScheduleResponse);
    rpc GetStatus(StatusRequest) returns (StatusResponse);
}

// Event Publishing
ContentPublishedEvent {
    Guid ContentId
    string NetworkType
    string PostId
    DateTime PublishedAt
    Dictionary<string, object> Metrics
}
```

### 2.2 Serviço: Data Collector Service

#### Responsabilidades
- Coletar métricas em real-time
- Agregação e normalização
- Stream processing
- Data enrichment

#### Componentes Técnicos
```csharp
// Stream Processing Pipeline
public class MetricsCollectionPipeline
{
    private readonly IDataflowBlock<CollectionRequest> _pipeline;
    
    public MetricsCollectionPipeline()
    {
        // Source: API Polling
        var pollBlock = new TransformBlock<CollectionRequest, RawMetrics>(
            async request => await PollNetworkAPI(request),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 10,
                BoundedCapacity = 100
            });
        
        // Transform: Normalize Data
        var normalizeBlock = new TransformBlock<RawMetrics, NormalizedMetrics>(
            metrics => NormalizeMetrics(metrics));
        
        // Enrich: Add Context
        var enrichBlock = new TransformBlock<NormalizedMetrics, EnrichedMetrics>(
            async metrics => await EnrichWithContext(metrics));
        
        // Sink: Store & Publish
        var storeBlock = new ActionBlock<EnrichedMetrics>(
            async metrics => await StoreAndPublish(metrics));
        
        // Link Pipeline
        pollBlock.LinkTo(normalizeBlock);
        normalizeBlock.LinkTo(enrichBlock);
        enrichBlock.LinkTo(storeBlock);
        
        _pipeline = pollBlock;
    }
}
```

#### Rate Limiting Strategy
```csharp
public class AdaptiveRateLimiter
{
    private readonly ConcurrentDictionary<string, TokenBucket> _buckets;
    
    public async Task<bool> TryAcquireAsync(string network, int tokens = 1)
    {
        var bucket = _buckets.GetOrAdd(network, CreateBucket);
        
        // Adaptive adjustment based on API responses
        if (bucket.LastResponseHeaders.ContainsKey("X-Rate-Limit-Remaining"))
        {
            bucket.AdjustCapacity(int.Parse(bucket.LastResponseHeaders["X-Rate-Limit-Remaining"]));
        }
        
        return await bucket.TryAcquireAsync(tokens);
    }
    
    private TokenBucket CreateBucket(string network) => network switch
    {
        "Twitter" => new TokenBucket(300, TimeSpan.FromMinutes(15)),
        "Instagram" => new TokenBucket(200, TimeSpan.FromHours(1)),
        "LinkedIn" => new TokenBucket(100, TimeSpan.FromDays(1)),
        "TikTok" => new TokenBucket(600, TimeSpan.FromMinutes(1)),
        _ => new TokenBucket(100, TimeSpan.FromHours(1))
    };
}
```

### 2.3 Serviço: Analytics Engine Service

#### Responsabilidades
- Processar métricas em batch/stream
- Calcular KPIs e scores
- Detectar padrões e anomalias
- Gerar insights com ML

#### Machine Learning Pipeline
```csharp
public class MLAnalyticsPipeline
{
    private readonly MLContext _mlContext;
    private readonly ITransformer _engagementModel;
    private readonly ITransformer _sentimentModel;
    private readonly ITransformer _anomalyModel;
    
    public async Task<AnalysisResult> AnalyzeAsync(MetricsData data)
    {
        var results = new AnalysisResult();
        
        // 1. Engagement Prediction
        var engagementPipeline = _mlContext.Transforms
            .NormalizeMinMax("Features")
            .Append(_mlContext.Regression.Trainers.FastTree());
            
        results.PredictedEngagement = await PredictEngagement(data);
        
        // 2. Sentiment Analysis
        results.Sentiment = await AnalyzeSentiment(data.Comments);
        
        // 3. Anomaly Detection
        var anomalyPipeline = _mlContext.Transforms
            .DetectAnomaliesBySrCnn("Features", confidence: 0.95);
            
        results.Anomalies = await DetectAnomalies(data);
        
        // 4. Time Series Forecasting
        var forecastPipeline = _mlContext.Forecasting
            .ForecastBySsa(horizon: 7, windowSize: 30);
            
        results.Forecast = await ForecastMetrics(data);
        
        return results;
    }
}
```

#### Complex Event Processing
```csharp
public class CEPEngine
{
    private readonly IStreamProcessingEngine _engine;
    
    public void ConfigureRules()
    {
        // Viral Detection Rule
        _engine.CreateRule("ViralDetection")
            .When(e => e.EngagementRate > e.AverageRate * 3)
            .And(e => e.SharesPerHour > 100)
            .Then(e => PublishEvent(new ViralContentDetected(e)));
        
        // Crisis Detection Rule
        _engine.CreateRule("CrisisDetection")
            .When(e => e.NegativeSentiment > 0.5)
            .And(e => e.CommentsPerMinute > 20)
            .And(e => e.UnfollowRate > 0.01)
            .Then(e => PublishEvent(new CrisisDetected(e)));
        
        // Opportunity Detection
        _engine.CreateRule("OpportunityDetection")
            .When(e => e.CompetitorEngagement < e.YourEngagement * 0.5)
            .And(e => e.TrendingTopicRelevance > 0.8)
            .Then(e => PublishEvent(new OpportunityDetected(e)));
    }
}
```

### 2.4 Serviço: Decision Engine Service

#### Responsabilidades
- Tomar decisões baseadas em regras
- Orquestrar ações automáticas
- Gerenciar workflows
- Otimizar estratégias

#### Rule Engine Implementation
```csharp
public class DecisionEngine
{
    private readonly RulesEngine _rulesEngine;
    
    public async Task<List<Decision>> EvaluateAsync(AnalysisContext context)
    {
        var decisions = new List<Decision>();
        
        // Load dynamic rules from database
        var rules = await LoadBusinessRules(context.AccountType);
        
        // Evaluate all rules in parallel
        var tasks = rules.Select(rule => EvaluateRule(rule, context));
        var results = await Task.WhenAll(tasks);
        
        // Priority-based decision making
        decisions = results
            .Where(r => r.Triggered)
            .OrderByDescending(r => r.Priority)
            .ThenByDescending(r => r.Confidence)
            .Take(5) // Max 5 decisions per evaluation
            .ToList();
        
        // Conflict resolution
        decisions = ResolveConflicts(decisions);
        
        return decisions;
    }
    
    private List<Decision> ResolveConflicts(List<Decision> decisions)
    {
        // Remove mutually exclusive decisions
        var groups = decisions.GroupBy(d => d.ResourceType);
        
        foreach (var group in groups)
        {
            if (group.Count() > 1)
            {
                // Keep only highest priority per resource
                var toKeep = group.OrderByDescending(d => d.Priority).First();
                decisions.RemoveAll(d => d.ResourceType == toKeep.ResourceType && d.Id != toKeep.Id);
            }
        }
        
        return decisions;
    }
}
```

#### Workflow Orchestration
```csharp
public class WorkflowOrchestrator
{
    private readonly IWorkflowEngine _engine;
    
    public async Task<WorkflowResult> ExecuteContentWorkflow(ContentRequest request)
    {
        var workflow = new ContentPublishingWorkflow
        {
            Steps = new[]
            {
                new ValidationStep(),
                new ContentOptimizationStep(),
                new ApprovalStep(request.RequiresApproval),
                new SchedulingStep(),
                new PublishingStep(),
                new MonitoringStep(),
                new AnalysisStep()
            }
        };
        
        var context = new WorkflowContext
        {
            Content = request.Content,
            Metadata = request.Metadata,
            CompensationEnabled = true // Enable rollback
        };
        
        return await _engine.ExecuteAsync(workflow, context);
    }
}
```

## 3. MODELO DE DADOS

### 3.1 Schema Principal (PostgreSQL)

```sql
-- Core Domain Tables
CREATE TABLE accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    type VARCHAR(50) NOT NULL, -- Individual, Agency, Enterprise
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    plan_tier VARCHAR(50) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE social_profiles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID REFERENCES accounts(id),
    network VARCHAR(50) NOT NULL,
    profile_id VARCHAR(255) NOT NULL,
    profile_username VARCHAR(255),
    access_token TEXT ENCRYPTED,
    refresh_token TEXT ENCRYPTED,
    token_expires_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(account_id, network, profile_id)
);

CREATE TABLE content (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID REFERENCES accounts(id),
    text TEXT,
    media_urls JSONB,
    hashtags TEXT[],
    mentions TEXT[],
    status VARCHAR(50) NOT NULL, -- Draft, Scheduled, Published, Failed
    scheduled_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE TABLE publications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    content_id UUID REFERENCES content(id),
    social_profile_id UUID REFERENCES social_profiles(id),
    network_post_id VARCHAR(255),
    published_at TIMESTAMP WITH TIME ZONE,
    status VARCHAR(50) NOT NULL,
    error_message TEXT,
    retry_count INT DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(content_id, social_profile_id)
);

-- Analytics Tables
CREATE TABLE metrics_snapshots (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    publication_id UUID REFERENCES publications(id),
    snapshot_time TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    impressions BIGINT,
    engagements BIGINT,
    likes BIGINT,
    comments BIGINT,
    shares BIGINT,
    saves BIGINT,
    clicks BIGINT,
    video_views BIGINT,
    completion_rate DECIMAL(5,2),
    sentiment_score DECIMAL(3,2),
    raw_metrics JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

CREATE INDEX idx_metrics_time ON metrics_snapshots(snapshot_time);
CREATE INDEX idx_metrics_pub ON metrics_snapshots(publication_id);

-- Time Series Table (Partitioned)
CREATE TABLE metrics_timeseries (
    time TIMESTAMPTZ NOT NULL,
    publication_id UUID NOT NULL,
    metric_name VARCHAR(50) NOT NULL,
    value DECIMAL,
    tags JSONB
) PARTITION BY RANGE (time);

-- Create monthly partitions
CREATE TABLE metrics_timeseries_2025_01 PARTITION OF metrics_timeseries
    FOR VALUES FROM ('2025-01-01') TO ('2025-02-01');

-- Hypertable for TimescaleDB (if using)
-- SELECT create_hypertable('metrics_timeseries', 'time');

-- Decision & Actions Tables
CREATE TABLE decisions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID REFERENCES accounts(id),
    trigger_type VARCHAR(100), -- Rule, ML, Manual
    trigger_data JSONB,
    decision_type VARCHAR(100),
    action_required TEXT,
    confidence DECIMAL(3,2),
    status VARCHAR(50), -- Pending, Approved, Rejected, Executed
    executed_at TIMESTAMP WITH TIME ZONE,
    result JSONB,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);

-- Audit & Compliance
CREATE TABLE audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    account_id UUID,
    user_id UUID,
    action VARCHAR(100) NOT NULL,
    resource_type VARCHAR(50),
    resource_id UUID,
    old_values JSONB,
    new_values JSONB,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

### 3.2 Analytics Database (MongoDB)

```javascript
// Social Interactions Collection
{
  _id: ObjectId,
  publicationId: UUID,
  interactionType: "comment|like|share|mention",
  userId: String,
  userName: String,
  userFollowers: Number,
  content: String,
  sentiment: Number, // -1 to 1
  timestamp: ISODate,
  parentId: ObjectId, // For replies
  metadata: {
    isVerified: Boolean,
    isInfluencer: Boolean,
    engagementRate: Number
  }
}

// Aggregated Analytics Collection
{
  _id: ObjectId,
  accountId: UUID,
  date: ISODate,
  network: String,
  metrics: {
    impressions: Number,
    reach: Number,
    engagement: {
      total: Number,
      likes: Number,
      comments: Number,
      shares: Number
    },
    audience: {
      demographics: {
        age: Map,
        gender: Map,
        location: Map
      },
      interests: [String]
    },
    performance: {
      engagementRate: Number,
      clickThroughRate: Number,
      conversionRate: Number
    }
  },
  topContent: [
    {
      contentId: UUID,
      engagementScore: Number,
      viralCoefficient: Number
    }
  ]
}

// ML Features Store
{
  _id: ObjectId,
  entityId: UUID,
  entityType: "account|content|campaign",
  features: {
    temporal: {
      hourOfDay: Number,
      dayOfWeek: Number,
      isWeekend: Boolean,
      isHoliday: Boolean
    },
    content: {
      textLength: Number,
      hashtagCount: Number,
      mentionCount: Number,
      mediaType: String,
      sentiment: Number
    },
    historical: {
      avgEngagement30d: Number,
      avgEngagement7d: Number,
      growthRate: Number,
      consistency: Number
    },
    computed: {
      viralProbability: Number,
      optimalPostTime: ISODate,
      expectedEngagement: Number
    }
  },
  computedAt: ISODate
}
```

### 3.3 Cache Strategy (Redis)

```redis
# Real-time Metrics
HSET metrics:{publicationId} impressions 10000 likes 500 comments 50
EXPIRE metrics:{publicationId} 3600

# Rate Limiting
ZADD ratelimit:{network}:{accountId} {timestamp} {requestId}
ZREMRANGEBYSCORE ratelimit:{network}:{accountId} -inf {cutoffTime}

# Session Cache
SET session:{sessionId} {userData} EX 1800

# Feature Flags
HSET features:{accountId} aiInsights "enabled" autoSchedule "disabled"

# Time Series Data (Redis TimeSeries)
TS.ADD engagement:{publicationId} * 0.045
TS.RANGE engagement:{publicationId} - + AGGREGATION avg 3600000

# Pub/Sub for Real-time Updates
PUBLISH updates:{accountId} {metricUpdate}

# Leaderboards
ZADD trending:{network} {score} {contentId}
ZREVRANGE trending:{network} 0 9 WITHSCORES

# Distributed Locks
SET lock:publish:{contentId} {workerId} NX EX 30
```

## 4. APIS E CONTRATOS

### 4.1 REST API Design

```yaml
openapi: 3.0.0
info:
  title: InfluenciAI API
  version: 1.0.0

paths:
  /api/v1/content:
    post:
      summary: Create content
      requestBody:
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/ContentRequest'
      responses:
        201:
          description: Content created
          headers:
            Location:
              schema:
                type: string
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ContentResponse'
                
  /api/v1/content/{id}/publish:
    post:
      summary: Publish content to networks
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: string
            format: uuid
      requestBody:
        content:
          application/json:
            schema:
              type: object
              properties:
                networks:
                  type: array
                  items:
                    type: string
                scheduleAt:
                  type: string
                  format: date-time
                options:
                  type: object
      responses:
        202:
          description: Publish request accepted
          
  /api/v1/analytics/realtime:
    get:
      summary: Get real-time analytics
      parameters:
        - name: publicationId
          in: query
          schema:
            type: string
            format: uuid
      responses:
        200:
          description: Real-time metrics
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/RealtimeMetrics'
```

### 4.2 g