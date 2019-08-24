To generate the code, run:

```sh
$$PROTOC -I . DrumFiles.proto  --csharp_out=. --csharp_opt=internal_access,file_extension=.g.cs
```

... with a suitable value for PROTOC.