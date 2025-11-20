# Build and Release Documentation

## Automated Build System - GitHub Actions

EncryptItVC uses GitHub Actions for continuous integration and automated building across all platforms.

### Build Workflows

#### 1. CI Workflow (`.github/workflows/ci.yml`)
- **Trigger**: Every push and pull request to `main` branch
- **Purpose**: Fast continuous integration testing
- **Platforms**: Ubuntu (cross-platform .NET builds)
- **Components**:
  - .NET 9 Server build verification
  - .NET 6 Windows Client build verification
  - Dependencies restoration and compilation testing

#### 2. Complete Build Workflow (`.github/workflows/build.yml`)
- **Trigger**: Push and pull request to `main` branch
- **Purpose**: Full platform builds with artifacts
- **Components**:

**Server Build (Ubuntu)**
- .NET 9.0 runtime
- Cross-platform compilation
- Automated testing (when tests available)

**Windows Client Build (Windows)**
- .NET 6.0 Desktop runtime
- WPF application compilation
- Published executable generation
- Artifact upload for distribution

**Android Mobile Build (Windows)**
- .NET 8.0 + MAUI workload
- Microsoft OpenJDK 17
- Android SDK automatic installation
- APK generation for distribution

**Release Packaging**
- Automated artifact collection
- Complete documentation bundling
- Build scripts and deployment tools
- Compressed release archive creation

### Build Verification

Each build includes:
- ✅ **Dependency Resolution**: All NuGet packages restored
- ✅ **Compilation Verification**: All projects compile without errors
- ✅ **Cross-Platform Compatibility**: Server builds on Linux, clients on Windows
- ✅ **Artifact Generation**: Deployable executables and packages created
- ✅ **Documentation Packaging**: Complete user and developer documentation

### Continuous Integration Benefits

1. **Code Quality Assurance**: Every commit is automatically tested
2. **Platform Compatibility**: Multi-OS build verification
3. **Release Automation**: Automatic artifact generation for distribution
4. **Dependency Management**: Automated package restoration and validation
5. **Build Reproducibility**: Consistent build environment across all runs

### SignPath Integration Ready

The GitHub Actions setup provides:
- **Automated Builds**: Required for SignPath OSS free code signing
- **Artifact Generation**: Consistent build outputs for signing
- **Version Control Integration**: Direct repository-to-build pipeline
- **Multi-Platform Support**: Windows executables and Android APKs
- **Professional CI/CD**: Industry-standard automated building practices

### Build Artifacts

Each successful build generates:
- **Windows Desktop Client**: `EncryptItVC.Client.exe` + dependencies
- **Android Mobile APK**: `com.encryptitvc.mobile.apk` (signed)
- **Server Binaries**: Cross-platform .NET 9 assemblies
- **Complete Release Package**: All components + documentation + build scripts

This automated build system ensures consistent, reproducible builds suitable for code signing and distribution through SignPath OSS free certification program.
