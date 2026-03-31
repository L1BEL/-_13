# ClothesAccessoriesApp

Структура проекта:

- `Models` — доменные сущности каталога и корзины.
- `Data` — `AppDbContext` и инициализация SQLite базы.
- `Repositories` — repository-слой для работы с EF Core.
- `Services` — корзина и переключение тем.
- `ViewModels` — MVVM-логика гостя и администратора.
- `Styles` — общие стили, светлая и темная темы.
- `tests/ClothesAccessoriesApp.Tests` — xUnit тесты для сервисов и репозиториев.
