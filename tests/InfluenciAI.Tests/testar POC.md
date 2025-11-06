 Como Rodar o POC - Passo a Passo

  Pré-requisitos

  1. .NET SDK 10 RC - O projeto requer especificamente a versão RC do .NET 10:
  # Verificar versão (você precisará instalar no WSL)
  dotnet --version
  1. Download: https://dotnet.microsoft.com/download/dotnet/10.0
  2. Docker Desktop - ✅ Já está instalado

  Passo 1: Subir as Dependências (Banco de Dados)

  # No diretório raiz do projeto
  docker compose up -d

  Isso irá iniciar:
  - PostgreSQL (porta 5432)
  - Redis (porta 6379)
  - RabbitMQ (porta 5672 + management UI na 15672)

  Aguarde cerca de 10-15 segundos para os containers ficarem prontos.

  Passo 2: Rodar a API

  dotnet run --project src/Server/InfluenciAI.Api

  A API estará disponível em:
  - http://localhost:5228
  - Swagger: http://localhost:5228/swagger

  Usuário de teste criado automaticamente:
  - Email: admin@local
  - Senha: Admin!234

  Passo 3: Rodar o Desktop (WPF)

  Em outro terminal:

  dotnet run --project src/Client/InfluenciAI.Desktop

  A janela do aplicativo WPF abrirá automaticamente.

  Testando o POC

  No Desktop:
  1. Faça login com admin@local / Admin!234
  2. Navegue pelas abas:
    - Tenants: Gerenciar inquilinos (criar, editar, deletar)
    - Users: Gerenciar usuários por tenant

  No Swagger (http://localhost:5228/swagger):
  1. Clique em "Authorize"
  2. Faça login via endpoint /auth/login para obter o token
  3. Teste os endpoints:
    - GET /tenants - Listar tenants
    - POST /tenants - Criar tenant
    - GET /health - Status da aplicação

  Verificar se está tudo rodando

  # Verificar containers
  docker ps

  # Verificar API
  curl http://localhost:5228/health

  ---
  Nota importante: Você está no WSL2 e o dotnet não foi encontrado. Você precisará instalar o .NET 10 RC no WSL antes de executar os passos acima. Alternativamente, pode     
  executar os comandos dotnet no PowerShell/CMD do Windows se já tiver o .NET instalado lá.

  Quer que eu te ajude a instalar o .NET SDK no WSL?

> eu vou abrir no visual studio. 

● Ótimo! Abrir no Visual Studio é a forma mais prática. Aqui está o passo a passo:

  Rodando pelo Visual Studio

  1. Subir as Dependências (Docker)

  Antes de abrir o Visual Studio, execute no terminal (pode ser PowerShell/CMD do Windows):

  # No diretório do projeto
  docker compose up -d

  Aguarde os containers subirem (PostgreSQL, Redis, RabbitMQ).

  2. Abrir a Solution

  Abra o arquivo:
  InfluenciAI.sln

  3. Configurar Multiple Startup Projects

  No Visual Studio:

  1. Clique com botão direito na Solution → Properties
  2. Vá em Startup Project
  3. Selecione Multiple startup projects
  4. Configure para iniciar:
    - ✅ InfluenciAI.Api → Start
    - ✅ InfluenciAI.Desktop → Start
  5. Clique em OK

  4. Rodar (F5)

  Pressione F5 ou clique em Start.

  Isso iniciará:
  - A API (com Swagger abrindo no navegador)
  - O aplicativo Desktop (janela WPF)

  5. Testar

  No Desktop:
  - Login: admin@local / Admin!234
  - Navegue pelas abas (Tenants, Users)

  No Swagger (navegador):
  - Teste os endpoints da API

  ---
  Troubleshooting

  Se der erro de banco de dados na primeira execução:
  - A API aplica as migrações automaticamente no ambiente Development
  - Verifique se os containers Docker estão rodando: docker ps

  Se precisar rodar migrations manualmente:
  dotnet ef database update --project src/Infra/InfluenciAI.Infrastructure --startup-project src/Server/InfluenciAI.Api

  Alguma dúvida ou precisa de ajuda com algum passo?