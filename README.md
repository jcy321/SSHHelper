# SSHHelper

一个基于 WPF 与 .NET 8 的 Windows SSH 密钥配置辅助工具。

## 功能特性

- 🔐 **授权验证**：支持在线/离线授权码验证，机器码绑定
- 🔑 **一键配置**：自动生成 SSH 密钥对并部署到远程服务器
- 📝 **SSH Config**：自动配置本地 `.ssh/config` 文件
- 🎨 **Material Design**：现代化的 UI 界面

## 技术栈

- .NET 8 + WPF
- Renci.SshNet (SSH 操作)
- CommunityToolkit.Mvvm (MVVM 框架)
- MaterialDesignThemes (UI 框架)
- Microsoft.Extensions.DependencyInjection (依赖注入)

## 项目结构

```
SSHHelper/
├── src/
│   ├── SSHHelper.Core/       # 核心库
│   │   ├── Services/         # SSH密钥生成、配置管理、远程部署
│   │   ├── Models/           # 数据模型
│   │   └── Helpers/          # 工具类
│   │
│   ├── SSHHelper.Auth/       # 授权模块（闭源核心）
│   │   ├── LicenseValidator  # 在线/离线授权验证
│   │   ├── SecureStorage     # DPAPI安全存储
│   │   └── MachineIdGenerator # 机器码生成
│   │
│   └── SSHHelper.App/        # WPF 应用
│       ├── ViewModels/       # 视图模型
│       ├── Views/            # XAML 视图
│       └── Services/         # 导航服务等
│
├── .github/workflows/        # GitHub Actions 自动构建
├── build.bat                 # Windows 本地构建脚本
└── README.md
```

## 构建方式

### 方式一：本地构建（需要 Windows）

前置条件：
- Windows 10/11
- .NET 8 SDK

```bash
# 克隆项目
git clone https://github.com/your-repo/SSHHelper.git
cd SSHHelper

# 方式1：使用构建脚本
build.bat

# 方式2：手动构建
dotnet restore
dotnet build --configuration Release
dotnet publish src/SSHHelper.App/SSHHelper.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 方式二：GitHub Actions 自动构建

1. Fork 或 Clone 本项目到你的 GitHub
2. 创建标签触发构建：
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. 在 GitHub Actions 中查看构建结果
4. 从 Releases 下载编译好的 exe 文件

## 使用说明

### 启动流程

1. 首次启动显示授权验证界面
2. 输入机器码获取授权码
3. 输入授权码验证
4. 验证成功后进入主界面

### SSH 密钥配置

1. 填写服务器信息：
   - **别名**：SSH config 中的 Host 名称
   - **机器标记**：服务器识别本机的标记（可选）
   - **IP 地址**：目标服务器 IP
   - **端口**：SSH 端口（默认 22）
   - **用户名**：默认 root
   - **密码**：root 密码（仅用于本次配置）

2. 点击「一键配置」

3. 配置完成后，使用以下命令连接：
   ```bash
   ssh <别名>
   ```

## 授权说明

- SSHHelper.Core：开源（MIT License）
- SSHHelper.Auth：闭源核心，需要授权码使用
- SSHHelper.App：部分开源

## 开发环境

- Visual Studio 2022 或 JetBrains Rider
- .NET 8 SDK
- Windows 10/11

## License

MIT License (Core 部分)