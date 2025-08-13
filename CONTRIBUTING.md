# Contributing to VoiceLive API Console Application

Thank you for your interest in contributing to this project! Here are some guidelines to help you get started.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/your-username/VoiceLiveAPISamples.git`
3. Create a new branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Test your changes
6. Commit your changes: `git commit -m "Add your feature"`
7. Push to your branch: `git push origin feature/your-feature-name`
8. Create a Pull Request

## Development Setup

### Prerequisites
- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Azure AI Foundry account (for testing)

### Building the Project
```bash
dotnet restore
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Running the Application
```bash
dotnet run
```

## Code Style Guidelines

- Use C# coding conventions
- Include XML documentation for public APIs
- Write unit tests for new functionality
- Keep methods focused and small
- Use meaningful variable and method names

## Pull Request Guidelines

1. **Description**: Provide a clear description of what your PR does
2. **Testing**: Include tests for new functionality
3. **Documentation**: Update documentation if needed
4. **Breaking Changes**: Clearly mark any breaking changes
5. **Dependencies**: Avoid adding unnecessary dependencies

## Types of Contributions

### Bug Fixes
- Include steps to reproduce the bug
- Add regression tests if possible
- Reference the issue number in your commit message

### New Features
- Discuss the feature in an issue first
- Include comprehensive tests
- Update documentation and examples
- Consider backward compatibility

### Documentation
- Fix typos and grammar
- Improve code examples
- Add missing documentation
- Translate documentation

### Performance Improvements
- Include benchmarks showing the improvement
- Ensure no regression in functionality
- Document any trade-offs

## Code Review Process

1. All contributions require review
2. Maintainers will review PRs as time permits
3. Address feedback promptly
4. Keep discussions respectful and constructive

## Reporting Issues

When reporting issues, include:
- Clear description of the problem
- Steps to reproduce
- Expected vs. actual behavior
- Environment details (.NET version, OS, etc.)
- Relevant logs or error messages

## Feature Requests

For feature requests:
- Search existing issues first
- Clearly describe the use case
- Explain why the feature would be valuable
- Consider proposing an implementation approach

## Communication

- Use GitHub Issues for bug reports and feature requests
- Use GitHub Discussions for questions and general discussion
- Be respectful and constructive in all interactions

## License

By contributing, you agree that your contributions will be licensed under the Boost Software License 1.0.

## Recognition

Contributors will be recognized in the README.md file and release notes.

Thank you for contributing!