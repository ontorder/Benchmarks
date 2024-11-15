## .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
```assembly
; test.field_access_bench.regular()
       mov       rax,235334043C8
       mov       rax,[rax]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       mov       ecx,[rax+10]
       add       ecx,[rax+14]
       add       ecx,[rax+18]
       add       ecx,[rax+1C]
       add       ecx,[rax+20]
       add       ecx,[rax+24]
       add       ecx,[rax+28]
       add       ecx,[rax+2C]
       mov       eax,ecx
       ret
; Total bytes of code 52
```

## .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
```assembly
; test.field_access_bench.ref_access()
       mov       rax,248ACC043C8
       mov       rax,[rax]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       add       rax,8
       mov       rax,[rax]
       mov       ecx,[rax+10]
       add       ecx,[rax+14]
       add       ecx,[rax+18]
       add       ecx,[rax+1C]
       add       ecx,[rax+20]
       add       ecx,[rax+24]
       add       ecx,[rax+28]
       add       ecx,[rax+2C]
       mov       eax,ecx
       ret
; Total bytes of code 55
```

## .NET 8.0.11 (8.0.1124.51707), X64 RyuJIT AVX2
```assembly
; test.field_access_bench.array_access()
       sub       rsp,28
       mov       rax,1CA170043C8
       mov       rax,[rax]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       mov       rax,[rax+8]
       mov       ecx,[rax+8]
       test      ecx,ecx
       je        short M00_L00
       mov       edx,[rax+10]
       cmp       ecx,1
       jbe       short M00_L00
       add       edx,[rax+14]
       cmp       ecx,2
       jbe       short M00_L00
       add       edx,[rax+18]
       cmp       ecx,3
       jbe       short M00_L00
       add       edx,[rax+1C]
       cmp       ecx,4
       jbe       short M00_L00
       add       edx,[rax+20]
       cmp       ecx,5
       jbe       short M00_L00
       add       edx,[rax+24]
       cmp       ecx,6
       jbe       short M00_L00
       add       edx,[rax+28]
       cmp       ecx,7
       jbe       short M00_L00
       add       edx,[rax+2C]
       mov       eax,edx
       add       rsp,28
       ret
M00_L00:
       call      CORINFO_HELP_RNGCHKFAIL
       int       3
; Total bytes of code 112
```

