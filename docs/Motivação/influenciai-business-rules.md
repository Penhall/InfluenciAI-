# Regras de Negócio Completas - InfluenciAI Platform

## 1. PERSONAS E NECESSIDADES PROFUNDAS

### 1.1 Persona: Criador de Conteúdo Individual
**Perfil Detalhado:**
- Faixa etária: 18-35 anos
- Volume de posts: 3-10 por dia
- Redes principais: Instagram, TikTok, Twitter
- Receita: Depende de engajamento para monetização
- Tempo disponível: 2-4h/dia para gestão de redes
- Dor principal: Não sabe qual conteúdo performa melhor

**Necessidades Específicas:**
1. **Timing Optimization**
   - Identificar horários de pico da audiência
   - Sugerir melhor momento para cada tipo de conteúdo
   - Alertar sobre saturação de posts

2. **Content Performance Analysis**
   - Qual formato gera mais engajamento (vídeo, foto, texto)
   - Comprimento ideal de vídeos
   - Hashtags mais efetivas
   - Palavras-chave que geram reação

3. **Audience Insights**
   - Demografia dos seguidores ativos
   - Interesses correlacionados
   - Horários de maior atividade
   - Taxa de crescimento orgânico

### 1.2 Persona: Social Media Manager (Agência)
**Perfil Detalhado:**
- Gerencia: 5-20 contas simultâneas
- Volume: 50-200 posts/dia totais
- Equipe: 2-10 pessoas
- Necessidade: ROI mensurável para clientes
- Reportes: Semanais e mensais obrigatórios

**Necessidades Específicas:**
1. **Multi-Account Management**
   - Dashboard unificado para todas as contas
   - Aprovação em cascata de conteúdo
   - Templates por indústria/nicho
   - Calendário editorial integrado

2. **Team Collaboration**
   - Workflow de criação → revisão → aprovação → publicação
   - Atribuição de tarefas por membro
   - Comentários e feedback internos
   - Versionamento de conteúdo

3. **Client Reporting**
   - Relatórios white-label personalizáveis
   - Comparação com concorrentes
   - ROI e conversões rastreadas
   - Projeções de crescimento

### 1.3 Persona: Empresa/Marca
**Perfil Detalhado:**
- Tamanho: 50-5000 funcionários
- Objetivo: Brand awareness + vendas
- Budget: R$ 10k-500k/mês em marketing digital
- Compliance: Necessita aprovações legais
- Métricas: CAC, LTV, conversão

**Necessidades Específicas:**
1. **Brand Safety**
   - Moderação automática de comentários
   - Detecção de crise de reputação
   - Respostas pré-aprovadas para FAQ
   - Escalação para time humano

2. **Sales Integration**
   - Tracking de conversão por post
   - Atribuição de vendas por campanha
   - Link com CRM (Salesforce, HubSpot)
   - Lead scoring de comentadores

3. **Compliance & Governance**
   - Approval workflow com legal
   - Audit trail completo
   - Retenção de dados conforme LGPD
   - Controle de acesso granular

### 1.4 Persona: Influenciador Profissional
**Perfil Detalhado:**
- Seguidores: 100k-10M
- Receita: R$ 50k-2M/mês
- Parcerias: 10-50 brands simultâneas
- Equipe: 2-20 pessoas
- Necessidade: Maximizar valor por post

**Necessidades Específicas:**
1. **Partnership Management**
   - Tracking de obrigações contratuais
   - Calendário de posts patrocinados
   - Métricas por parceria
   - Cálculo de media kit value

2. **Audience Quality**
   - Detecção de followers falsos
   - Engagement rate real vs bots
   - Geographic distribution
   - Purchasing power analysis

3. **Content Monetization**
   - Precificação dinâmica por post
   - A/B testing de CTAs
   - Link tracking para afiliados
   - Revenue per post analysis

## 2. REGRAS DE AUTOMAÇÃO DE COLETA E ENVIO

### 2.1 Regras de Publicação Multi-Rede

#### 2.1.1 Adaptação de Conteúdo por Rede
**Twitter/X:**
- Limite: 280 caracteres (ou threads)
- Se texto > 280: Automaticamente criar thread
- Imagens: Máx 4, otimizar para 16:9
- Vídeos: Máx 2:20, < 512MB
- Regra: Se contém link, colocar no final para não reduzir alcance
- Hashtags: Máximo 2, integradas no texto

**Instagram:**
- Feed: Quadrado (1:1) ou retrato (4:5)
- Stories: 9:16, máx 15 segundos/vídeo
- Reels: 9:16, 15-90 segundos
- Carrossel: Máx 10 imagens/vídeos
- Hashtags: 10-30, mix de populares e nicho
- Regra: Primeira frase deve capturar atenção (aparecer antes de "mais...")

**LinkedIn:**
- Texto: Até 3000 caracteres, mas mostrar apenas 210 iniciais
- Formato: Profissional, evitar emojis excessivos
- Horário: Dias úteis, 7-9h ou 17-18h
- Vídeos: Preferir nativos vs YouTube links
- Regra: Se B2B, incluir insights/dados

**TikTok:**
- Vídeo obrigatório: 15s-10min
- Aspecto: 9:16 vertical
- Música: Usar trending sounds
- Hashtags: 3-5, incluir challenges
- Regra: Primeiros 3 segundos críticos

**YouTube (Shorts/Regular):**
- Shorts: Vertical, máx 60s
- Regular: Horizontal, sem limite
- Thumbnail: Customizada obrigatória
- Descrição: Primeiras 125 caracteres vitais
- Tags: 10-15 relevantes

#### 2.1.2 Regras de Timing Inteligente
**Análise de Audiência:**
- Coletar dados de 30 dias de atividade
- Identificar top 3 horários por dia da semana
- Considerar timezone dos seguidores majoritários
- Ajustar para feriados e eventos

**Evitar Canibalização:**
- Mínimo 2 horas entre posts na mesma rede
- Máximo 3 posts/dia no Instagram feed
- Stories: Sem limite, mas espaçar 30 min
- Twitter: Máximo 15 tweets/dia

**Cross-posting Intelligence:**
- Delay de 15-30 min entre redes
- Ordem: Twitter → Instagram → LinkedIn → TikTok
- Razão: Twitter mais real-time, LinkedIn mais lento

### 2.2 Regras de Coleta de Dados

#### 2.2.1 Métricas Primárias (Coletadas em Real-Time)
**Engajamento Direto:**
- Views/Impressions: A cada 5 minutos primeiras 2 horas
- Likes: A cada 1 minuto primeira hora
- Comments: Instantâneo com análise de sentimento
- Shares/Retweets: Tracking de alcance viral
- Saves/Bookmarks: Indicador de valor

**Triggers de Coleta Intensiva:**
- Se engagement rate > 2x média: Coletar a cada 30s
- Se viral (>1000 shares/h): Stream contínuo
- Se crise (sentiment < -0.5): Alertas instantâneos

#### 2.2.2 Métricas Secundárias (Batch Collection)
**Perfil da Audiência:**
- Demografia: 1x/dia
- Interesses: 1x/semana
- Comportamento: 1x/semana
- Dispositivos: 1x/mês

**Competitive Intelligence:**
- Posts dos concorrentes: 2x/dia
- Engagement rate deles: 1x/dia
- Followers ganhos/perdidos: 1x/dia
- Hashtags trending no nicho: 4x/dia

#### 2.2.3 Regras de Rate Limiting
**Por Rede Social:**
- Twitter API: 300 requests/15min
- Instagram API: 200 requests/hora
- LinkedIn API: 100 requests/dia
- TikTok API: 600 requests/minuto

**Estratégia de Priorização:**
- Contas Premium: Coleta prioritária
- Contas em crise: Override de limites
- Contas dormentes: Coleta mínima
- Pool de tokens: Rotação automática

### 2.3 Regras de Resposta Automática

#### 2.3.1 Classificação de Interações
**Prioridade CRÍTICA (responder < 5 min):**
- Reclamações públicas
- Mentions de influenciadores (>10k followers)
- Perguntas sobre disponibilidade/preço
- Comentários virais negativos

**Prioridade ALTA (responder < 30 min):**
- Perguntas sobre produtos/serviços
- Feedback construtivo
- Pedidos de suporte
- Elogios de clientes verificados

**Prioridade MÉDIA (responder < 2h):**
- Comentários positivos genéricos
- Emojis reactions
- Perguntas já respondidas (link FAQ)
- Mentions neutras

**Prioridade BAIXA (responder < 24h):**
- Spam filtrado
- Comentários irrelevantes
- Auto-promocão de terceiros

#### 2.3.2 Templates Inteligentes
**Regras de Personalização:**
- Incluir nome do usuário se disponível
- Variar respostas (10 templates por tipo)
- Adaptar tom à rede (formal LinkedIn, casual Twitter)
- Incluir emoji apropriado ao contexto

**Escalação Automática:**
- Se sentiment muito negativo → Notificar gestor
- Se cliente VIP → Resposta personalizada humana
- Se questão legal → Bloquear resposta automática
- Se ameaça → Alertar segurança + screenshot

## 3. REGRAS DE ANÁLISE E INTELIGÊNCIA

### 3.1 Algoritmos de Scoring e Ranking

#### 3.1.1 Engagement Quality Score (EQS)
**Fórmula Base:**
```
EQS = (W1×Likes + W2×Comments + W3×Shares + W4×Saves) / (Impressions × TimeDecay)

Onde:
- W1 = 1 (peso base likes)
- W2 = 3 (comentários valem 3x mais)
- W3 = 5 (shares são mais valiosos)
- W4 = 4 (saves indicam intenção)
- TimeDecay = 1 + (HorasDesdePub / 24)
```

**Ajustes por Rede:**
- Instagram: Saves peso 6 (mais importante)
- Twitter: Retweets peso 7 (viralização)
- LinkedIn: Comments peso 5 (discussão profissional)
- TikTok: Completion Rate × 10 (assistir completo)

#### 3.1.2 Audience Value Score (AVS)
**Componentes:**
- Follower Authenticity: % de seguidores reais
- Engagement Consistency: Desvio padrão do engagement
- Growth Quality: Crescimento orgânico vs pago
- Geographic Relevance: % audiência no target
- Interest Alignment: Match com persona ideal

**Classificação:**
- AAA: AVS > 85 (Premium audience)
- AA: AVS 70-85 (High quality)
- A: AVS 55-70 (Good quality)
- B: AVS 40-55 (Average)
- C: AVS < 40 (Needs improvement)

#### 3.1.3 Content Performance Index (CPI)
**Análise Multidimensional:**
1. **Virality Potential**: Shares³ / Impressions
2. **Sentiment Score**: Positive - Negative / Total
3. **Conversion Rate**: Clicks / Impressions
4. **Longevity**: Engagements after 7 days / Total
5. **ROI**: (Revenue Generated - Cost) / Cost

### 3.2 Regras de Identificação de Padrões

#### 3.2.1 Padrões de Sucesso
**Detecção Automática:**
- Se 3 posts similares > 150% média engagement
- Identificar: Formato, horário, hashtags, comprimento
- Criar "Success Template" automático
- Sugerir replicação com variações

**Elementos Analisados:**
- Primeira frase/hook
- Tipo de mídia
- Call-to-action usado
- Emotional triggers
- Storytelling structure

#### 3.2.2 Padrões de Fracasso
**Red Flags:**
- 3 posts consecutivos < 50% média
- Queda de followers > 1%/dia
- Sentiment negativo > 40%
- Unfollow rate > follow rate

**Análise de Causa Raiz:**
- Mudança de algoritmo da rede
- Saturação de conteúdo
- Timing inadequado
- Competitor campaign
- Audience fatigue

### 3.3 Regras de Predição e Recomendação

#### 3.3.1 Predictive Analytics
**Modelo de Previsão de Engagement:**
- Base: Histórico 90 dias
- Variáveis: Dia, hora, tipo, tamanho, hashtags
- Método: Regressão + ML (Random Forest)
- Accuracy target: > 75%
- Update: Diário com novos dados

**Previsões Fornecidas:**
- Expected impressions ± 20%
- Expected engagement rate ± 15%
- Viral probability (0-100%)
- Best posting time (precisão 30 min)
- Optimal content type

#### 3.3.2 Sistema de Recomendações
**Recomendações Táticas (Curto Prazo):**
1. "Poste agora - audiência 40% mais ativa"
2. "Use vídeo - engagement 3x maior hoje"
3. "Evite links - algoritmo penalizando"
4. "Responda @user - influenciador importante"
5. "Delete post - sentiment muito negativo"

**Recomendações Estratégicas (Longo Prazo):**
1. "Migre foco para Reels - Stories saturados"
2. "Invista em TikTok - crescimento 300%/mês"
3. "Reduza frequência - audience fatigue detectada"
4. "Mude tom - audiência prefere casual"
5. "Teste novo horário - padrão mudou"

## 4. REGRAS DE OTIMIZAÇÃO E DECISÃO

### 4.1 Automação de Decisões

#### 4.1.1 Decisões Autônomas (Sem Aprovação)
**Permitidas Automaticamente:**
- Ajustar horário de post em ± 2 horas
- Responder comentários positivos
- Curtir menções da marca
- Adicionar hashtags trending relevantes
- Pausar post com sentiment < -0.7

**Logs Obrigatórios:**
- Timestamp da decisão
- Razão/trigger
- Impacto esperado
- Resultado real (follow-up)
- Reversibilidade

#### 4.1.2 Decisões Semi-Autônomas (Notifica e Executa)
**Executa mas Avisa:**
- Remarcar post para outro dia
- Desativar comentários (hate detected)
- Boost de budget em post viral
- Ativar respostas automáticas
- Mudar categoria de conteúdo

**Período de Reversão:**
- Usuario tem 1 hora para cancelar
- Notificação push + email + in-app
- Botão de "undo" prominente

#### 4.1.3 Decisões Assistidas (Requer Aprovação)
**Sugestões com Aprovação:**
- Deletar post permanentemente
- Mudar estratégia de conteúdo
- Investir budget pago
- Responder crise pública
- Bloquear usuários

### 4.2 Regras de Budget e ROI

#### 4.2.1 Alocação Inteligente de Budget
**Distribuição Dinâmica:**
- Se ROI rede > 150%: Aumentar 20%
- Se ROI rede < 50%: Reduzir 30%
- Se CPM subiu > 40%: Pausar e reavaliar
- Budget mínimo: 10% para testes
- Budget máximo: 40% em uma única rede

**Critérios de Investimento:**
- CPC < R$ 2,00: Green light
- CPM < R$ 30,00: Aprovado
- CAC < 30% LTV: Escalar
- ROAS > 3: Aumentar budget
- CTR < 0.5%: Revisar creative

#### 4.2.2 Cálculo de ROI Real
**Atribuição Multi-Touch:**
- First-touch: 10% crédito
- Mid-touches: 20% dividido
- Last-touch: 40% crédito
- View-through: 30% se < 24h

**Métricas Financeiras:**
- Revenue diretamente atribuível
- Pipeline influenced (B2B)
- Brand lift medido
- Saves para conversão futura
- LTV de novos seguidores

### 4.3 Regras de Gestão de Crise

#### 4.3.1 Detecção de Crise
**Triggers Automáticos:**
- 10+ comentários negativos/hora
- Perda de 1000+ followers/hora
- Mention por verified account negativa
- Hashtag negativa trending
- Article/news negativo publicado

**Níveis de Severidade:**
- DEFCON 5: Monitoring normal
- DEFCON 4: Increased monitoring
- DEFCON 3: Team alerted
- DEFCON 2: Crisis mode activated
- DEFCON 1: All hands, pause all posts

#### 4.3.2 Protocolo de Resposta
**Ações Imediatas (< 15 min):**
1. Pausar todas publicações agendadas
2. Screenshot de tudo
3. Alertar chain of command
4. Preparar holding statement
5. Ativar monitoring 24/7

**Comunicação de Crise:**
- Tom: Empático mas não admitir culpa
- Velocidade: < 1 hora primeiro statement
- Canal: Mesma rede onde começou
- Follow-up: A cada 2 horas updates
- Resolution: Post detalhado quando resolvido

## 5. MÉTRICAS E KPIs POR PERSONA

### 5.1 KPIs para Criador Individual
**Métricas Principais:**
- Follower Growth Rate: Target +10%/mês
- Engagement Rate: Target > 3%
- Reach Rate: Target > 50% followers
- Story Views: Target > 20% followers
- DM Response Rate: Target < 2 horas

**Métricas de Monetização:**
- CPM médio: Track evolução
- Sponsored Post Value: Calcular por post
- Affiliate Revenue: Por link/código
- Product Sales: Atribuição direta
- Live Gifts/Donations: Por stream

### 5.2 KPIs para Agência
**Métricas de Performance:**
- Client Retention: Target > 90%
- Campaign Success Rate: Target > 80%
- Average Client Growth: Target +20%/quarter
- Team Productivity: Posts/person/day
- Error Rate: Target < 1%

**Métricas Operacionais:**
- Time to Publish: Target < 5 min
- Approval Cycle: Target < 2 horas
- Report Generation: Target < 10 min
- Client Satisfaction: NPS > 70
- Profit Margin: Target > 40%

### 5.3 KPIs para Empresa
**Métricas de Negócio:**
- CAC via Social: Track por canal
- Social Commerce Revenue: GMV total
- Brand Sentiment: Target > +0.7
- Share of Voice: Target > 30%
- Crisis Response Time: Target < 30 min

**Métricas de Brand:**
- Brand Awareness Lift: Target +15%/year
- Consideration Rate: Target > 25%
- Purchase Intent: Target > 10%
- Net Promoter Score: Target > 50
- Customer Lifetime Value: Growth +20%

## 6. INTEGRAÇÕES E DADOS EXTERNOS

### 6.1 Fontes de Dados para Enriquecimento

#### 6.1.1 Dados de Mercado
**Ferramentas de Competitor Intelligence:**
- SEMrush: Keywords e tráfego
- SimilarWeb: Comportamento audiência
- BuzzSumo: Conteúdo viral no nicho
- Brandwatch: Mentions e sentiment
- Sprinklr: Enterprise social data

**Dados Coletados:**
- Share of voice por tópico
- Trending topics no segmento
- Campaigns dos concorrentes
- Influenciadores do nicho
- Benchmarks de engagement

#### 6.1.2 Dados de Audiência
**Enriquecimento Demográfico:**
- Clearbit: Dados empresa (B2B)
- FullContact: Dados pessoa
- Pipl: Social profiles
- DataAxle: Consumer data
- Experian: Credit/purchase power

**Informações Adicionadas:**
- Renda estimada
- Educação
- Interesses de compra
- Localização precisa
- Comportamento online

### 6.2 Integrações com Stack de Marketing

#### 6.2.1 CRM Integration
**Salesforce:**
- Lead creation de social
- Opportunity influence
- Contact enrichment
- Campaign attribution
- Social selling signals

**HubSpot:**
- Social inbox unificado
- Lead scoring com social
- Workflow triggers
- Content performance
- ROI tracking

#### 6.2.2 Analytics Platforms
**Google Analytics:**
- Social traffic attribution
- Conversion tracking
- Audience insights
- Behavior flow
- E-commerce tracking

**Adobe Analytics:**
- Cross-channel attribution
- Segment creation
- Predictive analytics
- Real-time dashboards
- Custom variables

### 6.3 Compliance e Governança

#### 6.3.1 LGPD/GDPR Compliance
**Regras de Dados Pessoais:**
- Consentimento explícito para coleta
- Direito ao esquecimento (delete em 30 dias)
- Portabilidade (export em 7 dias)
- Anonimização após 90 dias
- Audit log de todo acesso

**Retenção de Dados:**
- Posts públicos: Indefinido
- DMs/Privado: 90 dias
- Analytics agregado: 2 anos
- Dados pessoais: 1 ano ou até revogação
- Backups: 30 dias rolling

#### 6.3.2 Segurança e Acesso
**Níveis de Acesso:**
1. **Viewer**: Read-only dashboards
2. **Creator**: Create/edit próprio conteúdo
3. **Manager**: Approve/edit team content
4. **Admin**: All access + settings
5. **Owner**: Billing + delete account

**Autenticação e Autorização:**
- MFA obrigatório para Manager+
- SSO para empresas
- API keys com scope limitado
- Rate limiting por usuário
- Session timeout 30 min idle

## 7. MODELOS DE PRICING E LIMITES

### 7.1 Tiers de Serviço

#### Tier 1: Starter (Criador Individual)
**Limites:**
- 3 redes sociais
- 100 posts/mês
- 10k análises/mês
- 7 dias histórico
- Basic AI insights

**Preço:** R$ 97/mês

#### Tier 2: Professional (Influenciador)
**Limites:**
- 5 redes sociais
- 500 posts/mês
- 100k análises/mês
- 90 dias histórico
- Advanced AI
- A/B testing

**Preço:** R$ 397/mês

#### Tier 3: Business (PME)
**Limites:**
- 10 redes sociais
- 2000 posts/mês
- 500k análises/mês
- 1 ano histórico
- Premium AI
- 5 usuários
- API access

**Preço:** R$ 1.997/mês

#### Tier 4: Enterprise (Grandes Empresas)
**Limites:**
- Ilimitadas redes
- Ilimitados posts
- Ilimitadas análises
- Histórico completo
- Custom AI models
- Ilimitados usuários
- Dedicated support
- SLA 99.9%

**Preço:** Custom (R$ 10k+/mês)

### 7.2 Uso de Recursos e Throttling

**Rate Limits por Tier:**
- Starter: 10 requests/min
- Professional: 60 requests/min
- Business: 300 requests/min
- Enterprise: Unlimited

**Priorização de Processamento:**
- Enterprise: Real-time (< 1s)
- Business: Near real-time (< 10s)
- Professional: Fast (< 30s)
- Starter: Standard (< 2 min)

## 8. CASOS DE USO DETALHADOS

### 8.1 Fluxo: Lançamento de Produto

**Contexto:** Empresa vai lançar produto novo

**Pré-Lançamento (30 dias antes):**
1. Criar teaser campaign
2. Identificar influenciadores relevantes
3. Agendar countdown posts
4. Preparar FAQ responses
5. Setup monitoring keywords

**Dia D:**
1. Publicação sincronizada multi-rede
2. Live monitoring dashboard
3. Resposta instantânea a queries
4. Boost em posts com tração
5. Coleta de feedback real-time

**Pós-Lançamento (30 dias):**
1. Análise de sentimento
2. Compilação de testimonials
3. Identificação de pain points
4. Ajuste de mensagem
5. Report de sucesso

### 8.2 Fluxo: Gestão de Influenciador

**Discovery:**
1. Buscar por nicho/audiência
2. Analisar autenticidade
3. Verificar brand fit
4. Calcular estimated ROI
5. Gerar proposta

**Campanha:**
1. Brief automático
2. Content approval workflow
3. Publicação coordenada
4. Track performance real-time
5. Pagamento por performance

**Análise:**
1. ROI calculation
2. Audience overlap
3. Sentiment analysis
4. Recomendação de continuidade
5. Ranking de influenciadores

### 8.3 Fluxo: Crisis Management

**Detecção (Minuto 0):**
- Alert triggered
- Screenshot everything
- Pause scheduled posts
- Alert crisis team
- Open war room

**Resposta (Minutos 0-15):**
- Assess severity
- Draft holding statement
- Get legal approval
- Publish response
- Monitor reactions

**Resolução (Horas 1-24):**
- Detailed response
- Individual replies
- Media statement
- Internal comms
- Lesson learned doc

**Recovery (Dias 1-30):**
- Sentiment tracking
- Reputation rebuild
- Positive content push
- Influencer support
- Monthly report

## 9. ALGORITMOS DE OTIMIZAÇÃO

### 9.1 Content Mix Optimization

**Objetivo:** Encontrar mix ideal de conteúdo

**Variáveis:**
- % Educational content
- % Entertainment
- % Promotional
- % User-generated
- % Behind-scenes

**Método:**
- Multi-armed bandit algorithm
- Exploration vs exploitation
- Bayesian optimization
- Convergence em 30 dias

**Output:**
- Mix recomendado por dia da semana
- Ajuste por sazonalidade
- Personalização por audiência

### 9.2 Hashtag Optimization

**Descoberta:**
- Scraping de trending
- Análise de concorrentes
- Volume vs competition
- Relevance scoring

**Seleção:**
- 30% high volume (>100k posts)
- 50% medium (10k-100k)
- 20% niche (<10k)
- Evitar banned/shadowbanned
- Rotação para evitar spam flag

**Performance Tracking:**
- Impressions por hashtag
- Engagement lift
- Follower source
- Hashtag lifecycle

### 9.3 Timing Optimization

**Coleta de Dados:**
- Follower online times
- Historical engagement
- Competition posting times
- Algorithm preferences
- Time zone distribution

**Modelo Preditivo:**
- Time series analysis
- Seasonal decomposition
- Day-of-week effects
- Holiday adjustments
- Event detection

**Recomendação:**
- Top 3 slots por dia
- Confidence interval
- Expected reach
- Competition density
- Algorithm boost periods
