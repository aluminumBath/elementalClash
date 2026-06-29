# Unity Error Intake Checklist

Use this when Unity reports compile or playtest errors.

## Copy errors from Console

```text
1. Open Unity Console.
2. Turn off Collapse if you need full repeated details.
3. Click the first real C# compile error.
4. Copy:
   - full error code
   - file path
   - line/column
   - message
5. Repeat for the first 10 unique errors.
```

## Group errors by kind

```text
1. Missing type / namespace
2. Missing method or overload
3. Missing enum member
4. Serialized field/property name changed
5. Duplicate type
6. Assembly/reference problem
7. UI prefab/template problem
8. Runtime null reference
```

## First-pass fix order

```text
1. Duplicate type errors
2. Missing enum/type errors
3. Missing method signature errors
4. Editor generator errors
5. Runtime/UI prefab errors
6. Test-only errors
```

## What to send back

```text
- first 10 unique Console errors
- Unity version
- whether import finished
- whether menus appeared
- whether Build Rounded Playable Scene ran
- whether EditMode or PlayMode tests were run
```
