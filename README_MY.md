##
ЦЕЛЬ:
Доступ пользователю в режиме самообслуживания по модели SaaS.
Управлять отоплением, включать и выключать свет, запирать и отпирать автоматические ворота, удаленно наблюдать за домом и будущее неуточненное поведение.
Пользователь самостоятельно выбирает модули, их подключает, настраивает сценарии работы и просматривает телеметрию.
Поддержка подключения к экосистеме устройств партнеров по стандартным протоколам.
(Веб-разработка готова)

ТРЕБВАНИЯ:
Модули управления приборами и приборы должны быть независимыми и продаваться в отдельных комплектах.
Устройства должны быть доступны через интернет.
Пользователь могет настраивать сценарии работы приборы и модулей.

##
ФУНКЦИОНАЛЬНОСТЬ:

Управление отоплением: 
удалённо включение/выключение отопления;
устанавка температуры;
автоматическая поддержка заданной температуры.
Мониторинг температуры:
получает данных о температуре с датчиков, установленных в домах, и их публикация через веб-интерфейс.

##
АНАЛИЗ АРХИТЕКТУРЫ МОНОЛИТА

Команда разработчиков (5 человек)
Команда DevOps (2 человека)
Команда QA (3 человека)
Команда по обслуживанию клиентов (10 человек)
Команда по продажам и маркетингу (5 человек)
Java, PostgreSQL, Монолитная, Синхронное, Развертывание: Требует остановки всего приложения.
Слабой стороной приложения является раздутость штата.
Сильной стороной приложения является его востребованность (заказ).

##
ДОМЕНЫ И ГРАНИЦЫ КОНТЕКСТОВ

Домен: планировщик работы модулей
сущности: модуль, расписание, план температур, провайдер услуг;
объекты-значения: время включения/выключения, температура;
агрегаты: расписание;
репозитории: репозиторий расписания и плана;
сервисы: сервис планирования расписания и плана, сервис управления услугами, сервис управления модулями.

поддомен управления отоплением
поддомен управление светом
поддомен управление автоматическими воротами
поддомен управление камерами
поддомен будущее неуточненное поведение
контекст: ощий поддоменный API

Домен: мониторинг
сущности: просмотр, уведомелние;
объекты-значения: способы уведомелний;
агрегаты: просмотр;
репозитории: репозиторий просмотра и уведомелний;
сервисы: сервис просмотра, сервис уведомелний.

поддомен видеонаблюдение:
контекст: просмотр онлайн и архива поддомена камеры.
поддомен просмотыр телеметрии:
контекст: датички поддоменов отопления, света, ворот, камер.

##
МИКРОСЕРВИСЫ

Микросервис планирования работы модулей
Контекст: планирование работой модуля в соответствии с расписанием и планом с использованием провайдеров услуг.

Микросервис взаимодействия с модулями
Контекст: предоставление услуг устройств с помощью модулей модули.

Микросервис мониторинга
Контекст: просмотр (веб) онлайн и архив камер.

Микросервис уведомлений
Контекст:  управление уведомлениями от датчиков

Микросервис учетной записи
Контекст: аутентификация, авторизация, техподдержка.

Микросервис покупок
Контекст: подписка.

##
C4 ДИАГРАММА (System Context diagram)
User — пользователь системы, мониторит, настривает.
Administrator — администратор, управляющий системой и осуществляющий поддержку пользователей.
EcoSystem — основная система.
Third-Party API — внешний API устройств.
Bank System — внешняя банковская система.


##
Подготовите план перехода к микросервисной архитектуре, который будет использоваться в следующих подзаданиях.
Диаграмма создана с использованием PlantUML и соответствует стандартам C4.


## Подзадание 1.3: ER-диаграмма
Основные сущности и атрибуты:
 User:
  UserId
  Token
  PaymentId
  DeviceIds
  TaskIds
  Subscription
  MetaInfo (Address, Birthday...)
  Transactions

 Device:
  DeviceId
  OwnerUserId
  IpPort
  ModuleId
  HWID
  Status
  Params
  Metrics
  Address
  Type
  ThirdPartyAPI

 Module
  ModuleId
  DeviceIds
  TaskIds
  Interfaces

 Task
  TaskId
  UserId
  DeviceId
  ModuleId
  Info
  Shedule
  States

## Описание связей:
User }o--|| Device, User }o--|{ Task : один к многим.
Device ||--o{ Module, Task |o--|| Device : один к одному.
Module }o--|| Task : один к многим.


## Подзадание 1.4: Создание и документирование API
Сложность бизнес функций была оценена как низкая. 
Количество клиентов исчисляется сотнями - малая загруженность. 
Состав команды исчисляется несколькими человек - крайне мало. 
Исходя из вышесказаннаго было решено сделать проект максимально простым в разработке и развертывании.
Поэтому микросервисы спроектированы максимально независимо - между ними нет API взаимодействия. Вся синхронизация происходит из БД.
Но в качестве выполнения задания был выбран наипростейший REST API:

# Profile <- REST -> Billing
Покупка:
Эндпойнт: /Billing/MakePay
Описание: Возвращает информацию об результате транзакции
Метод: POST
Формат запроса: userid:int64, itemid:int64, paymentid:int64
Формат ответа: transaction:json {success:bool, payload:json, itemid:uint, userid:uint, subscription:json}
Коды ответа: 200 — успех, 400 — неверный ввод, 500 — внутренняя ошибка
Пример: запрос = userid:123, itemid:321, paymentid:1; ответ = 200 OK success:false, payload:2L32KFCVK123KVSD, itemid:321, userid:123, subscription:null, error:"pleasecomeagain"

Обновление доступных способов оплаты:
Эндпойнт: /Billing/GetOptions
Описание: Позволяет узнать доступные способы оплаты исходя из входных данных.
Метод: GET
Формат запроса:
Формат ответа: paymentslist:json
Коды ответа: 200 — успех, 500 — внутренняя ошибка
Пример: ответ = 200 OK commandslist:"[0:payment1,1:payment2...]", success:true, error:null

# Sheduler  <- REST -> ModulesController
Обновление доступных комманд:
Эндпойнт: /ModulesController/GetCommands
Описание: Позволяет узнать доступные команды.
Метод: GET
Формат запроса: 
Формат ответа: commandslist:json
Коды ответа: 200 — успех, 500 — внутренняя ошибка
Пример: ответ = 200 OK commandslist:"[0:openbigdoor,1:swithlightbadroom,2:settemperature1floor...]", success:true, error:null

Обновление доступных модулей:
Эндпойнт: /ModulesController/GetModules
Описание: Позволяет узнать доступные модули.
Метод: GET
Формат запроса: 
Формат ответа: moduleslist:json
Коды ответа: 200 — успех, 500 — внутренняя ошибка
Пример: ответ = 200 OK commandslist:"[0:openbigdoor,1:swithlight,2:settemperature...]"

Выполнение команды:
Эндпойнт: /ModulesController/{command}
Описание: Выполняет команду над устройством.
Метод: POST
Формат запроса: userid:int64, params:json, deviceid:int64
Формат ответа: success:bool, metrics:json, status:string
Коды ответа: 200 — успех, 404 — устройство не найдено, 500 — внутренняя ошибка, 202 - неверные параметры
Пример: запрос settemperature = userid:123, params:"[temperature:25, datetime:\"now\"]", deviceid:321; ответ = 200 OK success:true, metrics:"[temperature:23]", status:"icqking", error:null


## Подзадание 2.1
Микросервисы (в миникубе) нужно инициализировать данными репозитория базы данные через эндпоинт InitRepository, подробнее смотри в эндпоинтке 32222или32111/swagger/index.html (доступ только по HTTP).
Возможно придется сделать kubectl port-forward svc/svc-device-controller 32222:27222, kubectl port-forward svc/svc-metrics-controller 32111:27111
Не забыть развернуть Mongo sharding.
Пример:
http://localhost:32222/api/Device/InitRepository/svc-db1-data:27020?databaseName=somedb&databaseCollection=Metrics'

# DeviceController (соответственно заданию 2.1, а не основной архитектуре)
Получение/установка статуса устройства:
Эндпойнт: /Device/{deviceId}/Status
Описание: Получение/установка статуса устройства.
Метод: PUT
Формат запроса: deviceId:int64, status:string
Формат ответа: success:bool, metrics:json
Коды ответа: 200 — успех, 404 — устройство не найдено, 500 — внутренняя ошибка, 202 - неверные параметры
Пример: запрос /Device/123/Status?status=on = deviceId:123, status:"on"; ответ = 200 OK success:true, metrics:"[deviceId=123, temperature:23, status:"on"]"

# MetricsController (соответственно заданию 2.1, а не основной архитектуре)
Обработка телеметрии устройства:
Эндпойнт: /Metrics/Telemetry
Описание: Сохраняет телеметрию в бд
Метод: POST
Формат запроса: deviceId:int64, temperature:float
Формат ответа: success:bool, metrics:json
Коды ответа: 200 — успех, 404 — устройство не найдено, 500 — внутренняя ошибка, 202 - неверные параметры
Пример: запрос /Metrics/Telemetry?deviceId=123&temperature=33 = deviceId:123, temperature:33; ответ = 200 OK success:true, metrics:"[deviceId=123, temperature:23, status:"on"]"


## Подзадание 2.2 (Так как необязательное)
Сделано через Kubernetes Services service-metrics и service-device в режиме NodePort.
При миникубе нужно сделать перенаправление портов:
kubectl port-forward svc/svc-device-controller 32222:27222
kubectl port-forward svc/svc-metrics-controller 32111:27111

