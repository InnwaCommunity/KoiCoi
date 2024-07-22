                                    KoiCoi
KoiCoi ဟာငွေကြေးစုဆောင်းမှုကို support ပေးသော software ဖြစ်ပါတယ်။

Features





```bash
dotnet ef dbcontext scaffold "Server=NYEINCHANNMOE;Database=Koi_Coi;User Id=sa;Password=nyein@8834;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o AppDbContextModels -c AppDbContext -f

Scaffold-DbContext "Server=NYEINCHANNMOE;Database=Koi_Coi;User ID=sa; Password=nyein@8834;Integrated Security=True;Trusted_Connection=true;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir AppDbContext -Tables Tbl_AdminUserLogin -f
```

```bash
Encryption.EncryptID(item.UserId.ToString(), LoginUserId.ToString())
Convert.ToInt32(Encryption.DecryptID(id, LoginEmpID.ToString()))
```

result pattern
```bash
Result<T>.Success(data);
Result<T>.Error(error);
```


