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

## 使用注意

1. 在当前项目的数据库表 model 中，不要添加非当前项目的导航属性，否则会导致迁移报错