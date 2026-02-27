How caching is implemented

- A new `ICacheService` interface exposes `GetAsync`, `SetAsync`, `GetVersionAsync`, and `BumpVersionAsync`.
- `RedisCacheService` (StackExchange.Redis) implements `ICacheService` and stores JSON strings.
- A simple versioning per-resource (`<prefix>:version`) is used to compose cache keys, enabling easy invalidation by bumping the version.

Usage guidance (example to cache doctors list):

- Compose key: `doctors:list:{version}:{page}:{size}` where `version` is obtained from `GetVersionAsync("doctors:list")`.
- On reads: try `GetAsync<List<DoctorInfoDTO>>(key)`; if exists return cached value; otherwise query DB, then `SetAsync`.
- On writes (add/update/delete doctor): call `BumpVersionAsync("doctors:list")` to invalidate caches.

Run with docker-compose

- Start Redis and API: `docker-compose up --build`
- Ensure `.env` has `Redis__Connection=redis:6379` if running inside docker-compose
