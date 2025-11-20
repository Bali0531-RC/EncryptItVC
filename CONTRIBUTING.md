# Contributing to EncryptItVC

Thank you for your interest in contributing to EncryptItVC! This document provides guidelines for contributing to our secure voice communication platform.

## 🚀 Quick Start

1. **Fork** the repository
2. **Clone** your fork locally
3. **Create** a feature branch: `git checkout -b feature/your-feature-name`
4. **Make** your changes
5. **Test** your changes with our automated build system
6. **Commit** with descriptive messages
7. **Push** to your fork
8. **Create** a Pull Request

## 🏗️ Development Setup

### Prerequisites
- .NET 9.0 SDK (for Server)
- .NET 6.0 SDK (for Windows Client)  
- .NET 8.0 SDK + MAUI workload (for Mobile Client)
- Visual Studio 2022 or VS Code
- Git

### Local Build Testing
```bash
# Test Server build
cd Server
dotnet build --configuration Release

# Test Windows Client build  
cd Client
dotnet build --configuration Release

# Test Android Mobile build (Windows only)
cd MobileClient
dotnet build -c Release -f net8.0-android
```

## 🔧 Project Structure

- `Server/` - .NET 9 cross-platform voice server
- `Client/` - WPF .NET 6 Windows desktop client
- `MobileClient/` - .NET MAUI Android mobile client
- `docs/` - Documentation and guides
- `.github/workflows/` - GitHub Actions CI/CD

## 📋 Contribution Guidelines

### Code Style
- Follow standard C# coding conventions
- Use meaningful variable and method names
- Include XML documentation for public APIs
- Maintain consistent indentation (4 spaces)

### Commit Messages
- Use clear, descriptive commit messages
- Start with a capital letter
- Use present tense ("Add feature" not "Added feature")
- Reference issues when applicable

### Pull Requests
- Keep PRs focused on a single feature or fix
- Include tests when applicable
- Update documentation for new features
- Ensure all GitHub Actions builds pass

## 🧪 Testing

Our GitHub Actions CI/CD automatically tests:
- ✅ All platform builds (Windows, Linux, Android)
- ✅ Dependency resolution
- ✅ Compilation verification
- ✅ Cross-platform compatibility

## 🐛 Bug Reports

When reporting bugs, please include:
- Operating system and version
- .NET version
- Steps to reproduce
- Expected vs actual behavior
- Error messages or logs

## 💡 Feature Requests

We welcome feature suggestions! Please:
- Check existing issues first
- Describe the use case clearly
- Explain how it benefits users
- Consider implementation complexity

## 📄 License

By contributing, you agree that your contributions will be licensed under the MIT License.

## 🤝 Code of Conduct

- Be respectful and inclusive
- Focus on constructive feedback
- Help others learn and grow
- Maintain a welcoming environment

## 📞 Getting Help

- **GitHub Issues**: Bug reports and feature requests
- **Discussions**: General questions and ideas
- **Documentation**: Check `README.md`, `INSTALL.md`, `USAGE.md`

Thank you for helping make EncryptItVC better! 🎙️
