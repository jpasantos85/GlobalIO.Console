### 📋 Project Overview / Visão Geral do Projeto

A powerful console application built with .NET 9 for interactive management and exploration of Google Cloud Pub/Sub. Supports both production Google Cloud services and local Docker emulator environments.
Uma aplicação console poderosa desenvolvida com .NET 9 para gerenciamento interativo e exploração do Google Cloud Pub/Sub. Suporta tanto serviços de produção do Google Cloud quanto ambientes de emulador Docker local.

### 🚀 Key Features / Funcionalidades Principais

- **📊 Discovery & Monitoring / Descoberta e Monitoramento**
  - List all topics and subscriptions / Listar todos os tópicos e subscriptions
  - View subscription details and configurations / Visualizar detalhes e configurações de subscriptions
  - Connection health checks / Verificações de saúde da conexão
  - Real-time statistics / Estatísticas em tempo real

- **💬 Message Management / Gerenciamento de Mensagens**
  - Peek messages without acknowledgment / Visualizar mensagens sem confirmação (peek)
  - Publish messages to topics / Publicar mensagens em tópicos
  - Clear messages from subscriptions / Limpar mensagens de subscriptions
  - View message attributes and metadata / Visualizar atributos e metadados das mensagens

- **🔧 Administration / Administração**
  - Create and delete topics / Criar e excluir tópicos
  - Manage subscriptions / Gerenciar subscriptions
  - Bulk operations / Operações em lote
  - Configuration management / Gerenciamento de configurações

### 🛠 Technical Stack / Stack Técnica

- **Framework**: .NET 9.0
- **UI**: Spectre.Console (Interactive CLI)
- **Pub/Sub**: Google.Cloud.PubSub.V1
- **Configuration / Configuração**: Microsoft.Extensions (appsettings.json, Environment Variables)
- **Container**: Docker Emulator Support / Suporte a Emulador Docker
- **Architecture / Arquitetura**: Dependency Injection, Repository Pattern / Injeção de Dependência, Padrão Repository

### 🎯 Ideal Use Cases / Casos de Uso Ideais

- 🏢 **Local Development / Desenvolvimento Local** - Full Pub/Sub functionality without cloud costs / Funcionalidade completa do Pub/Sub sem custos de cloud
- 🧪 **Integration Testing / Testes de Integração** - Reliable testing with emulator / Testes confiáveis com emulador
- 🔍 **Debugging** - Inspect messages and topics in real-time / Inspecionar mensagens e tópicos em tempo real
- 📚 **Learning / Aprendizado** - Explore Pub/Sub concepts and API / Explorar conceitos e API do Pub/Sub

---

## 🔧 Configuration / Configuração

### Environment Variables / Variáveis de Ambiente

```bash
# English
PUBSUB_EMULATOR_HOST=localhost:8681

# Português
PUBSUB_EMULATOR_HOST=localhost:8681
```

### AppSettings Example / Exemplo de AppSettings

```json
{
  "GooglePubSub": {
    "ProjectId": "my-project",
    "EmulatorHost": "localhost:8681",
    "UseEmulator": true,
    "TimeoutSeconds": 60,
    "MaxMessages": 10
  }
}
```

## 🎮 Interactive Features / Funcionalidades Interativas

| Feature / Funcionalidade | Description / Descrição |
|-------------------------|------------------------|
| 🎨 **Colorful UI** / **Interface Colorida** | Interactive menus with rich formatting |
| ⚡ **Real-time Operations** / **Operações em Tempo Real** | Immediate feedback for all actions |
| 🛡 **Error Handling** / **Tratamento de Erros** | Graceful error recovery and user guidance |
| 🔄 **Auto-retry** / **Tentativa Automática** | Smart retry logic for transient failures |

## 🌟 Coming Soon / Em Breve

> **Stay tuned for exciting updates!** / **Fique ligado para atualizações emocionantes!**
> 
> We're continuously working to enhance your Pub/Sub management experience with:
> / Estamos trabalhando continuamente para melhorar sua experiência de gerenciamento do Pub/Sub com:
> 
> - 🔐 **Enhanced Security** / **Segurança Aprimorada**
> - 🎨 **Richer Visualizations** / **Visualizações Mais Ricas**
> - ⚡ **Performance Optimizations** / **Otimizações de Performance**
> - 🔧 **Advanced Tooling** / **Ferramentas Avançadas**

## 📞 Support / Suporte

This project is maintained for educational and development purposes. / Este projeto é mantido para fins educacionais e de desenvolvimento.

**Happy coding! / Bons desenvolvimentos!** 🚀

---
# Google Pub/Sub Manager with Emulator / Gerenciador Google Pub/Sub com Emulador

## 🚧 Future Improvements / Melhorias Futuras

### 🔐 Authentication Support / Suporte de Autenticação
**Planned for future releases / Planejado para versões futuras:**
- 🔑 **Google Cloud Authentication** / Autenticação Google Cloud
  - Service account key support / Suporte a chaves de service account
  - OAuth 2.0 integration / Integração OAuth 2.0
  - ADC (Application Default Credentials) / Credenciais Padrão de Aplicação
  - Multiple project management / Gerenciamento de múltiplos projetos

### 🎨 Enhanced UI/UX / Interface Aprimorada
**Coming in next updates / Próximas atualizações:**
- 🌈 **Advanced Visualizations** / Visualizações Avançadas
  - Real-time metrics dashboard / Dashboard de métricas em tempo real
  - Message flow diagrams / Diagramas de fluxo de mensagens
  - Interactive charts and graphs / Gráficos interativos
  - Dark/Light theme support / Suporte a temas claro/escuro

### ⚡ New Features / Novas Funcionalidades
**Roadmap highlights / Destaques do roadmap:**
- 📊 **Advanced Monitoring** / Monitoramento Avançado
  - Message throughput analytics / Análise de throughput de mensagens
  - Subscription lag monitoring / Monitoramento de lag em subscriptions
  - Performance metrics / Métricas de performance
  - Alerting system / Sistema de alertas

- 🔄 **Message Operations** / Operações de Mensagem
  - Bulk message publishing / Publicação em lote de mensagens
  - Message filtering and search / Filtro e busca de mensagens
  - Message replay functionality / Funcionalidade de replay de mensagens
  - Dead letter queue management / Gerenciamento de dead letter queues

- 🛠 **Developer Tools** / Ferramentas de Desenvolvimento
  - Import/export configurations / Importar/exportar configurações
  - Template system for topics/subscriptions / Sistema de templates
  - API client code generation / Geração de código cliente da API
  - Integration testing utilities / Utilitários para testes de integração