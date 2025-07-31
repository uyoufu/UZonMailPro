# 开发文档

## 数据迁移

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