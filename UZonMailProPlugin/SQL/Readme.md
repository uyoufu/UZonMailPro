# �����ĵ�

## ����Ǩ��

### �ű�Ǩ��

``` powershell
z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin\Scripts
./add-migrations.ps1 -Name xxx
```

### �ֶ�ִ��


z D:\Develop\Personal\UZonMailProPlugins\UZonMailProPlugin

1. Mysql

dotnet ef migrations add fixAnchorCallbackFail --context MysqlContextPro --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add fixAnchorCallbackFail --context SqLiteContextPro --output-dir Migrations/SqLite -v

3. PostgreSQL

dotnet ef migrations add fixAnchorCallbackFail --context PostgreSqlContextPro --output-dir Migrations/PostgreSql -v

## ȡ������Ǩ��

1. Mysql

dotnet ef migrations remove --context MysqlContextPro -v

2. SqLite

dotnet ef migrations remove --context SqLiteContextPro -v

3. PostgreSQL

dotnet ef migrations remove --context PostgreSqlContextPro -v

## ʹ��ע��

1. �ڵ�ǰ��Ŀ�����ݿ�� model �У���Ҫ��ӷǵ�ǰ��Ŀ�ĵ������ԣ�����ᵼ��Ǩ�Ʊ���