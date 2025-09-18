# 🤝 Contributing to VHouse

## 🌱 Contributing to Animal Liberation Through Code

VHouse exists to serve real businesses promoting veganism: **Mona la Dona**, **Sano Market**, and **La Papelería**. Every contribution should advance this mission.

## 🎯 Contribution Philosophy

### Before Contributing, Ask:
1. **¿Cómo esto ayuda a los animales?** - How does this help animals?
2. **¿Resuelve un problema real de nuestros clientes?** - Does this solve a real client problem?
3. **¿Hace el sistema más confiable para Bernard?** - Does this make the system more reliable for Bernard?

### We Welcome:
- ✅ Bug fixes that improve reliability for real users
- ✅ Features that directly serve Mona la Dona, Sano Market, or La Papelería
- ✅ Performance improvements that reduce server costs
- ✅ Security enhancements that protect customer data
- ✅ Documentation that helps other developers understand the mission

### We Don't Accept:
- ❌ Over-engineering that delays real impact
- ❌ Features without clear user stories from real clients
- ❌ Changes that increase complexity without proportional value
- ❌ Academic exercises that don't serve the mission

## 🔧 Development Setup

### Prerequisites
```bash
# Required tools
dotnet --version  # Should be 8.0+
docker --version
git --version
```

### Local Setup
```bash
# 1. Fork and clone
git clone https://github.com/YOUR_USERNAME/VHouse.git
cd VHouse

# 2. Setup environment
cp .env.example .env
# Edit .env with your local settings

# 3. Start dependencies
docker-compose up -d postgres redis

# 4. Setup database
dotnet ef database update --project src/VHouse.Infrastructure

# 5. Build and test
dotnet build
dotnet test

# 6. Run application
dotnet run --project VHouse.Web
```

## 📝 Code Standards

### Commit Messages
Use conventional commits to maintain clear history:

```bash
feat: add bulk order import for Mona la Dona
fix: resolve tenant isolation bug in product queries
docs: update deployment guide for kubernetes
refactor: simplify order processing pipeline
test: add integration tests for multi-tenant scenarios
```

### Code Style
Follow the existing patterns:
- **Clean Architecture**: Domain → Application → Infrastructure → Web
- **CQRS**: Commands for writes, Queries for reads
- **Tenant Isolation**: Always filter by TenantId
- **Security First**: Validate all inputs, protect all outputs

### Branch Naming
```bash
feature/bulk-order-import
fix/tenant-isolation-bug
docs/kubernetes-deployment
refactor/order-processing
```

## 🧪 Testing Requirements

### Required Tests
All contributions must include appropriate tests:

```csharp
// Unit tests for business logic
[Test]
public void CreateProduct_WithValidData_ShouldSucceed()
{
    // Arrange
    var command = new CreateProductCommand
    {
        Name = "Organic Almond Milk",
        TenantId = "mona-la-dona"
    };

    // Act & Assert
    await handler.Handle(command);
}

// Integration tests for multi-tenancy
[Test]
public async Task GetProducts_ShouldOnlyReturnTenantProducts()
{
    // Ensure Mona la Dona only sees their products
    var products = await handler.Handle(new GetProductsQuery
    {
        TenantId = "mona-la-dona"
    });

    Assert.That(products.All(p => p.TenantId == "mona-la-dona"));
}
```

### Test Categories
```bash
# Run specific test categories
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
dotnet test --filter Category=Security
```

## 🔒 Security Guidelines

### Mandatory Security Checks
- ✅ All inputs validated and sanitized
- ✅ SQL injection prevention (parameterized queries)
- ✅ XSS prevention (output encoding)
- ✅ Tenant isolation verified in all data operations
- ✅ No secrets in code (use configuration/key vault)

### Security Testing
```csharp
// Example: Tenant isolation test
[Test]
public async Task Should_NotAllowCrossTenantDataAccess()
{
    // Attempt to access Sano Market data with Mona la Dona credentials
    var query = new GetProductsQuery { TenantId = "sano-market" };

    // Should throw UnauthorizedAccessException
    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        () => handler.Handle(query, monaLaDonaContext));
}
```

## 📋 Pull Request Process

### 1. Pre-submission Checklist
- [ ] Tests pass: `dotnet test`
- [ ] Build succeeds: `dotnet build --configuration Release`
- [ ] Security scan clean: No high/critical vulnerabilities
- [ ] Documentation updated if needed
- [ ] Real user story identified (which client benefits?)

### 2. PR Description Template
```markdown
## 🎯 User Story
As **Mona la Dona**, I need [functionality] so that [business value].

## 🔧 Technical Changes
- Brief description of implementation
- Key files modified
- Any breaking changes

## 🧪 Testing
- [ ] Unit tests added/updated
- [ ] Integration tests verified
- [ ] Manual testing completed

## 🔒 Security Considerations
- Input validation implemented
- Tenant isolation verified
- No secrets exposed

## 📊 Impact
- Performance impact: [None/Positive/Negative with details]
- Breaking changes: [None/List them]
- Migration needed: [No/Yes with steps]
```

### 3. Review Process
1. **Automated Checks**: CI pipeline must pass
2. **Code Review**: Focus on mission alignment and security
3. **Manual Testing**: Verify with real-world scenarios
4. **Approval**: Bernard's approval required for merge

### 4. Merge Requirements
- ✅ All CI checks passing
- ✅ At least one approval from @bernardoarancibia
- ✅ Up-to-date with master branch
- ✅ Squash merge preferred (clean history)

## 🚀 Release Process

### Version Strategy
- **Major**: Breaking changes (rare, coordinate with Bernard)
- **Minor**: New features that serve real clients
- **Patch**: Bug fixes and small improvements

### Release Notes
Document impact for real users:
```markdown
## v1.2.0 - 2024-09-18

### 🎯 For Mona la Dona
- ✨ Bulk order import reduces processing time by 75%
- 🔧 Fixed inventory sync delay issue

### 🎯 For Sano Market
- ✨ Advanced product filtering for better customer experience
- 🔧 Improved performance on large catalogs

### 🛡️ Security
- Updated dependencies to address CVE-2024-XXXX
- Enhanced tenant isolation verification
```

## 💬 Communication

### Getting Help
- **Questions**: GitHub Discussions
- **Bugs**: GitHub Issues with reproduction steps
- **Security**: Email bernard.uriza@vhouse.app (private)

### Community Guidelines
- Be respectful and constructive
- Focus on the mission: helping animals through better business tools
- Assume positive intent
- Provide context with your questions

## 🌱 Recognition

Contributors who advance the animal liberation mission will be recognized in:
- Release notes
- Project documentation
- Annual impact reports

Remember: every line of code is an act of love for animals and the businesses that serve them.

---

**🐄 Code for the animals. Build for the future. Ship for impact.**