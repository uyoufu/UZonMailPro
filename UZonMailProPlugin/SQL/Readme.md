# 开发文档

## 数据迁移

### 脚本迁移

``` powershell
z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin\Scripts
./add-migrations.ps1 -Name xxx
```

### 手动执行


z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add fixAnchorCallbackFail --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add fixAnchorCallbackFail --context SqLiteContextPro --output-dir Migrations/SqLite -v

3. PostgreSQL

dotnet ef migrations add fixAnchorCallbackFail --context PostgreSqlContextPro --output-dir Migrations/PostgreSql -v

## 取消数据迁移

1. Mysql

dotnet ef migrations remove --context MysqlContextPro -v

2. SqLite

dotnet ef migrations remove --context SqLiteContextPro -v

3. PostgreSQL

dotnet ef migrations remove --context PostgreSqlContextPro -v