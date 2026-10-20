# Sistema de Chamados (Help Desk)

API RESTful de gerenciamento de chamados técnicos desenvolvida em **ASP.NET Core 10** como projeto de portfólio para demonstrar habilidades em desenvolvimento backend com C#.

## 🎯 Objetivo

Criar uma API para gestão de chamados de suporte técnico, permitindo que usuários abram, acompanhem e resolvam tickets de forma organizada, aplicando boas práticas de desenvolvimento e arquitetura em nuvem.

## 🚀 Tecnologias Utilizadas

- **.NET 10** - Framework principal
- **ASP.NET Core Web API** - Construção da API REST com Controllers
- **Entity Framework Core** - ORM para acesso a dados
- **SQL Server** - Banco de dados relacional (local: Redragon\SQLEXPRESS)
- **Azure Table Storage** - Armazenamento de logs de auditoria (Azurite em desenvolvimento)
- **Git/GitHub** - Controle de versão
- **C#** - Linguagem de programação
- **xUnit** - Testes unitários (planejado)
- **JWT** - Autenticação (planejado)

## 📋 Funcionalidades (MVP)

### Funcionalidades Planejadas
- [ ] Cadastro de usuários (Solicitante, Atendente, Gestor)
- [ ] Abertura de chamados com título, descrição, categoria e prioridade
- [ ] Listagem de chamados com filtros (status, categoria, data)
- [ ] Atualização de status com máquina de estados (Aberto → Em Andamento → Resolvido → Fechado)
- [ ] Atribuição de atendentes aos chamados
- [ ] Histórico de comentários por chamado
- [ ] Prioridade de chamados (Baixa, Média, Alta, Urgente)
- [ ] Log de auditoria no Azure Table Storage (ChamadoLog)

### Status Atual
- ✅ Setup inicial do projeto .NET 10
- ✅ Configuração do Git e repositório no GitHub
- 🔄 Em desenvolvimento: Modelagem das entidades

## 🏗️ Arquitetura do Projeto

```
SistemaChamados/
├── Controllers/          # Endpoints da API
├── Models/               # Entidades (Usuario, Chamado, ChamadoLog, etc.)
├── Data/                 # DbContext e configurações
├── Services/             # Regras de negócio (futuro)
├── DTOs/                 # Objetos de transferência (futuro)
└── Program.cs            # Configuração da aplicação
```

## 🔧 Como Rodar o Projeto

### Pré-requisitos
- .NET 10 SDK instalado
- SQL Server (instância local ou Azure SQL)
- Azurite (para emular Azure Table Storage em desenvolvimento)
- Git

### Passos

1. **Clone o repositório**
```bash
git clone https://github.com/leandrovilar/sistema-chamados-helpdesk.git
cd sistema-chamados-helpdesk/SistemaChamados
```

2. **Configure a connection string**
Edite o arquivo `appsettings.json` e atualize a connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=Redragon\\SQLEXPRESS;Database=SistemaChamados;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

3. **Inicie o Azurite (para logs no Table Storage)**
```bash
azurite
```

4. **Aplique as migrations**
```bash
dotnet ef database update
```

5. **Execute o projeto**
```bash
dotnet run
```

6. **Acesse a API**
- Swagger UI: `https://localhost:7000/swagger` (após configurar Swagger)
- Ou teste via Postman/Insomnia

## 📚 Estrutura das Entidades (Planejado)

### Usuario
- Id (GUID), Nome, Email, Papel (Solicitante/Atendente/Gestor)

### Chamado
- Id (GUID), Titulo, Descricao, Status, Prioridade, SolicitanteId, AtendenteId (nullable), DataAbertura, DataFechamento (nullable)

### ChamadoLog (Azure Table)
- PartitionKey, RowKey, Timestamp, TipoAcao, ChamadoId, Detalhe (JSON)

### Máquina de Estados do Status
```
Aberto → EmAndamento
EmAndamento → Resolvido ou Aberto
Resolvido → Fechado
Fechado → (nenhuma mudança permitida)
```

## 🎓 Aprendizados em Andamento

Este projeto está sendo desenvolvido como parte de um processo de aprendizado em:
- Padrões de projeto em C# e ASP.NET Core
- Entity Framework Core (Code First, Migrations)
- API REST com Controllers
- Integração com Azure Table Storage para logs
- Git Flow e versionamento com commits convencionais
- Boas práticas de desenvolvimento backend
- Deploy no Azure (App Service + SQL Database + Storage Account)

## 📈 Próximos Passos (Roadmap)

1. ✅ Setup do projeto e Git
2. 🔄 Modelagem das entidades (Usuario, Chamado, ChamadoLog)
3. ⏳ Criar DbContext e primeira migration
4. ⏳ CRUD de Usuario
5. ⏳ CRUD de Chamado com máquina de estados
6. ⏳ Integração com Azure Table para logs
7. ⏳ Autenticação JWT
8. ⏳ Testes unitários com xUnit
9. ⏳ Configurar Swagger/OpenAPI
10. ⏳ Deploy no Azure

## 👨‍💻 Autor

**Leandro Braga Vilar**  
Desenvolvedor em formação | Técnico em Manutenção de Computadores  
📍 Cidade Ocidental, Goiás, Brasil  
🔗 [GitHub](https://github.com/leandrovilar)

---

**Status do Projeto**: Em Desenvolvimento 🚧  
**Última Atualização**: 28 de Setembro de 2026  
**Próxima Milestone**: Modelagem das entidades e primeira migration