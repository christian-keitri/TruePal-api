# ⚠️ TESTING IS MANDATORY ⚠️

## The Rule is Simple:

```
┌─────────────────────────────────────────────────────┐
│                                                     │
│   When you ADD or MODIFY a feature,                │
│   you MUST update tests.                           │
│                                                     │
│   NO TESTS = NO MERGE                              │
│                                                     │
└─────────────────────────────────────────────────────┘
```

## Quick Reference:

📖 **Full Testing Guide**: [TESTING_GUIDE.md](TESTING_GUIDE.md)  
📋 **Quick Template**: See [TESTING_GUIDE.md - Test Template](TESTING_GUIDE.md#-test-template)  
✅ **Requirements**: [CODING_STANDARDS.md - Rules 27-34](CODING_STANDARDS.md#-testing-requirements)

## Before Every Commit:

```bash
cd TruePal.Api.Tests
dotnet test
```

✅ All tests MUST pass

## Minimum Coverage:

For each feature:
- ✅ CREATE: 2 tests minimum
- ✅ READ: 3 tests minimum  
- ✅ UPDATE: 2 tests minimum
- ✅ DELETE: 2 tests minimum

## Test Naming:

`MethodName_ExpectedBehavior_StateUnderTest`

Example:
```csharp
CreateUser_ShouldSucceed_WhenValidDataProvided
GetUserById_ShouldReturnNull_WhenUserNotFound
```

---

**Remember**: Code without tests is broken by design.
