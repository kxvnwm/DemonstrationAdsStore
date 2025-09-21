# DemonstrationAdsStore

Демонстрационный сервис для хранения и поиска рекламных площадок по регионам (локациям).
Локации имеют иерархическую структуру (`/ru/svrd/revda` вложен в `/ru/svrd`, который вложен в `/ru`).
Рекламная площадка действует во всех указанных локациях и их дочерних регионах.

---

## Запуск

### 1. Клонирование репозитория

```bash
git clone <repo-url>
```

### 2. Запуск проекта

Перейти в папку решения и выполнить:

``` bash
dotnet run --project DemonstrationAdsStore
```

Приложение поднимется по умолчанию на:
http://localhost:5241

В режиме "Development" доступен Swagger UI.

---
## REST API

### Загрузка данных

`POST /api/ads/load`

Загружает файл со списком площадок и перезаписывает хранилище.
Принимает `multipart/form-data` с файлом `file`.

Пример:

``` bash
curl -X POST -F "file=@sites.txt" http://localhost:5241/api/ads/load
```

Ответ:

``` json
{
  "message": "Loaded",
  "advertisers": 4
}
```

#### Формат входного файла

``` txt
Поисковый.Директ:/ru
Особый рабочий:/ru/svrd/revda,/ru/svrd/pervik
Газета:/ru/msk,/ru/permobl,/ru/chelobl
Особая реклама:/ru/svrd
```

### Получение списка площадок

`GET /api/ads?location=<локация>`

Возвращает список площадок для заданной локации.
Учитывается вложенность (родительские регионы тоже применяются).

Примеры:

Для `/ru/msk`

``` bash
curl "http://localhost:5241/api/ads?location=/ru/msk"
```

Ответ:

``` json
[
  "Газета",
  "Поисковый.Директ"
]
```

Для `/ru`

``` bash
curl "http://localhost:5241/api/ads?location=/ru"
```

Ответ:

``` json
[
  "Поисковый.Директ"
]
```

---
## Тестирование

Юнит-тесты находятся в проекте `DemonstrationAdStoreTests`.

Запуск тестов:

``` bash
dotnet test
```
