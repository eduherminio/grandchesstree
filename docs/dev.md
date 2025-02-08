```
docker-compose up
dotnet run


```

```
docker buildx create --use 
ocker buildx build --platform linux/amd64,linux/arm64 -t aptacode/grand-chess-tree-worker:latest -t aptacode/grand-chess-tree-worker:0.0.2 -f .\GrandChessTree.Client\Dockerfile --push .
docker buildx imagetools inspect aptacode/grand-chess-tree-worker:latest
```