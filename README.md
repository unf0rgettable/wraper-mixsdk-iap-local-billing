# Модуль аналитики

Данный модуль предоставляет возможность легко интегрировать в проект аналитику.

Поддерживаемые платформы: Android, iOS. Может работать в редакторе. 


# Оглавление

- [Зависимости и требования](#зависимости-и-требования)
  * [Импорт](#импорт)
- [Инстанцирование необходимых сервисов](#инстанцирование-необходимых-сервисов)
- [Первичная настройка](#первичная-настройка)
- [События](#события)

## Зависимости и требования

Минимальная версия Unity <b> 2021.1.6f1 </b>

### Импорт

1. #### загрузите все эти архивы с сайта [Google API](https://developers.google.com/unity/archive#external_dependency_manager_for_unity) в <b>.tgz</b> формате.

* [com.google.firebase.app](https://developers.google.com/unity/archive#firebase_app_core)
* [com.google.firebase.auth](https://developers.google.com/unity/archive#firebase_authentication)
* [com.google.firebase.storage](https://developers.google.com/unity/archive#cloud_storage_for_firebase)
* [com.google.firebase.analytics](https://developers.google.com/unity/archive#google_analytics_for_firebase)
* [com.google.firebase.crashlytics](https://developers.google.com/unity/archive#firebase_crashlytics)
* [com.google.firebase.remote-config](https://developers.google.com/unity/archive#firebase_remote_config)

2. #### Создайте папку <b>GooglePackages</b> в корневой папке проекта *(в которой находится папка Assets)* и переместите эти архивы туда.

3. #### Если в проекте уже есть <b>ExternalDependencyManager</b>, то следует предварительно его выпилить.
Это делается удалением папки *ExternalDependencyManager* в папке *Assets*.

4. #### Добавьте следующие зависимости в свой manifest.json file *(он находится в папке Package)*.

```json 
 "dependencies": {
  "com.littlebitgames.environmentcore": "https://github.com/LittleBitOrganization/evolution-engine-environment-core-module.git#",
  "com.gameanalytics.sdk": "7.3.20",
  "com.google.external-dependency-manager": "https://github.com/LittleBitOrganization/evolution-engine-google-version-handler.git#1.2.171",
  "com.google.firebase.app": "file:../GooglePackages/com.google.firebase.app-10.1.0.tgz",
  "com.google.firebase.auth": "file:../GooglePackages/com.google.firebase.auth-10.1.0.tgz",
  "com.google.firebase.storage": "file:../GooglePackages/com.google.firebase.storage-10.1.0.tgz",
  "com.google.firebase.analytics": "file:../GooglePackages/com.google.firebase.analytics-10.1.0.tgz",
  "com.google.firebase.crashlytics": "file:../GooglePackages/com.google.firebase.crashlytics-10.1.0.tgz",
  "com.google.firebase.remote-config" : "file:../GooglePackages/com.google.firebase.remote-config-10.1.0.tgz",
  "com.littlebitgames.analytics": "https://github.com/LittleBitOrganization/evolution-engine-analytics.git",
  "com.dbrizov.naughtyattributes": "https://github.com/dbrizov/NaughtyAttributes.git#upm",
  "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm"
}
```

5. #### Добавьте в scopedRegistries внутри этого-же manifest.json следующие скоупы.
```json
"scopedRegistries": [
    {
      "name": "Game Package Registry by Google", 
      "url": "https://unityregistry-pa.googleapis.com/", 
      "scopes": [ 
        "com.google" 
      ]
    },
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.gameanalytics"
      ]
    }
  ]
```
6. #### Откройте Unity и дождитесь импорта зависимостей :raised_hands:
7. #### Создайте папку StreamingAssets в папке Assets и положите туда файл google-service.json который нужно взять из Firebase

## Инстанцирование необходимых сервисов
</b> Пример инсталлера, если используется Zenject</b>.

Инстанцирование сервисов без Zenject происходит аналогично.

```c#
public override void InstallBindings()
{
    Container
        .BindInterfacesAndSelfTo<EventsService>()
        .AsSingle()
        .NonLazy(); 

    Container
        .Bind<AnalyticsInitializer>()
        .FromNewComponentOnNewGameObject()
        .AsSingle()
        .NonLazy();
}  
```
## Первичная настройка

- В начале следует создать конфиг файл 


![Alt text](https://github.com/LittleBitOrganization/documentation-resources/blob/master/evolution-engine-analytics/documentation-images/1.jpg)

- Файл конфигурации отвечает за то, какие аналитики использовать в проекте. Данная настройка выставляется в созданном ScriptableObject'e по пути *Assets/Resources/Configs*

![Alt text](https://github.com/LittleBitOrganization/documentation-resources/blob/master/evolution-engine-analytics/documentation-images/2.jpg)


## События

Всего имеется 4 вида событий, код которых расположен в своих каталогах по пути *Runtime/EventSystem/Events*.

#### 1. EventAdImpression
Событие для отслеживания дохода с рекламы

#### 2. EventCurrency
Событие которое отслеживает события на транзакциях

#### 3. EventDesign
Событие для различных игровых ситуаций

#### 4. EventEcommerce
Для отслеживания игровых покупок


## Методы EventsService
- #### Метод для отправки информации о потраченных ресурсах
```c# 
SpendVirtualCurrency(DataEventCurrency dataEventCurrency, EventsServiceType flags = EventsServiceType.Everything) 
```
Пример использования
```c#
var eventData =
new DataEventCurrency(command.GetResourceId().Replace("resources/", ""), command.GetValue(),
transactionEventData.ItemType,
                    transactionEventData.ItemId);
Action logAction = command is RevenueCommand
  ? () => _eventsService.EarnVirtualCurrency(eventData)
  : () => _eventsService.SpendVirtualCurrency(eventData);
```


- #### Метод для отправки информации о полученных ресурсах
```c# 
EarnVirtualCurrency(DataEventCurrency dataEventCurrency, EventsServiceType flags = EventsServiceType.Everything)
```
Пример использования
```c#
var data = new DataEventCurrency(resource.Key.Replace("resources/", ""), resource.Value, "OnlineIncome", "all_islands");
_eventsService.EarnVirtualCurrency(data);
```


- #### Метод для отправки информации о игровом событии
```c#
DesignEvent(DataEventDesign dataEventDesign, EventsServiceType flags = EventsServiceType.Everything) 
```
Пример использования
```c#
private void SendEvent(string message)
{
  _eventsService.DesignEvent(new DataEventDesignWithParams(message));
}
```

- #### Метод для отправки информации о игровом событии с параметрами
```c# 
DesignEventWithParams(DataEventDesignWithParams dataEventDesignWithParams, EventsServiceType flags = EventsServiceType.Everything) 
```
Пример использования
```c#
private void LogEvent(QuestDescription questDescription)
{
  var questKeyParam = new EventParameter("quest_key", questDescription.Key);
  _eventsService.DesignEventWithParams(new DataEventDesignWithParams("quest_completed",
  questKeyParam + _paramsFactory.Create()));
}
```


- #### Метод для отправки информации о просмотре рекламы
```c# 
AdRevenuePaidEvent(IDataEventAdImpression data) 
```
 Пример использования
```c#
public AnalyticsAdsImpression(EventsService eventsService, Ads.Ads ads)
{
  ads.OnAdRevenuePaidEvent += eventsService.AdRevenuePaidEvent;
}
```
 
 
 
 
 





