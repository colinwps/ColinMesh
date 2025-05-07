# ColinMesh

[English](#colinmesh-in-english) | [中文说明](#colinmesh-项目说明)

---

## 🌐 ColinMesh 项目说明

ColinMesh 是一个使用 **ASP.NET 8.0** 构建的微服务架构后端项目，采用模块化设计，包含以下核心模块：

- 🔀 **网关模块（Gateway）**：负责统一入口、服务路由与负载均衡，支持 Ocelot/YARP。
- 🔐 **认证模块（Auth）**：基于 JWT 或 OAuth2，实现用户认证、授权和 Token 管理。
- 📦 **WebAPI 模块（Api）**：提供业务功能 API，采用 RESTful 设计，支持版本控制和 Swagger 文档。

### 📁 项目结构

```
ColinMesh/
├── ColinMesh.Gateway       # 网关服务
├── ColinMesh.Auth          # 认证服务
├── ColinMesh.Api           # Web API 服务
├── ColinMesh.Shared        # 通用库与接口定义
└── README.md               # 项目说明文档
```

### 🚀 快速开始

1. 克隆项目

```bash
git clone https://github.com/your-org/ColinMesh.git
cd ColinMesh
```

2. 构建与运行（需安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)）

```bash
dotnet build
dotnet run --project ColinMesh.Gateway
```

3. 访问地址

- 网关入口：http://localhost:8000
- Swagger 文档：http://localhost:8000/swagger

---

## 🌍 ColinMesh in English

**ColinMesh** is a backend microservices project built with **ASP.NET 8.0**, structured with modular components to support scalability and clean separation of concerns.

### 🔧 Modules Overview

- 🔀 **Gateway Module**: Acts as the unified entry point using Ocelot or YARP, handling routing, forwarding, and load balancing.
- 🔐 **Auth Module**: Handles authentication and authorization, supporting JWT or OAuth2.
- 📦 **WebAPI Module**: Implements business logic APIs, follows RESTful conventions, and supports Swagger.

### 📁 Project Structure

```
ColinMesh/
├── ColinMesh.Gateway       # Gateway Service
├── ColinMesh.Auth          # Auth Service
├── ColinMesh.Api           # Web API Service
├── ColinMesh.Shared        # Shared Libraries and Interfaces
└── README.md               # Project Description
```

### 🚀 Getting Started

1. Clone the repository

```bash
git clone https://github.com/your-org/ColinMesh.git
cd ColinMesh
```

2. Build and Run (Make sure [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) is installed)

```bash
dotnet build
dotnet run --project ColinMesh.Gateway
```

3. Visit Swagger UI

- Gateway: http://localhost:8000  
- Swagger: http://localhost:8000/swagger

---

## 📌 TODOs

- [ ] 容器化支持（Docker Compose）
- [ ] 单元测试与集成测试覆盖
- [ ] CI/CD 集成
- [ ] 支持多环境配置（开发、测试、生产）

## 📄 License

## 📢 许可协议

本项目采用 [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/) 协议，仅限于**个人学习和研究用途**，禁止任何形式的商业用途。如需商用，请联系作者获取授权。
