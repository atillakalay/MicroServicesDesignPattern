# ASP.NET Core 8.0 ile Mikroservis Tasarım Desenleri

## Repo Tanıtımı

Bu repoda, mikroservis mimarilerinde kullanabileceğimiz beş temel tasarım desenini öğreneceksiniz. Tüm örnekler **ASP.NET Core API (Net 8.0)** kullanılarak geliştirilmiştir.

## Kapsanan Tasarım Desenleri

### 1. Saga Design Pattern

**Saga Design Pattern**, dağıtık işlem senaryolarında mikroservisler arasında veri tutarlılığını yönetmenin bir yoludur. Bir saga, her hizmeti güncelleyen ve bir sonraki işlem adımını tetiklemek için bir mesaj veya olay yayımlayan işlem dizisidir. Bir adım başarısız olursa, saga, önceki işlemleri dengeleyen telafi işlemlerini gerçekleştirir.

### 2. Event Sourcing Pattern

Bir alandaki verilerin sadece mevcut durumunu saklamak yerine, bu verilere yapılan tüm eylemleri kaydetmek için yalnızca eklemeli bir mağaza kullanın. Mağaza, kayıt sistemi olarak işlev görür ve alan nesnelerini somutlaştırmak için kullanılabilir. Bu, karmaşık alanlardaki görevleri basitleştirerek veri modeli ve iş alanını senkronize etme ihtiyacını ortadan kaldırabilir, performansı, ölçeklenebilirliği ve yanıt verebilirliği artırabilir. Ayrıca, işlemsel veriler için tutarlılık sağlayabilir ve telafi edici eylemleri mümkün kılan tam denetim izlerini ve geçmişi koruyabilir.

### 3. Retry Pattern

Bir uygulamanın, bir hizmete veya ağ kaynağına bağlanmaya çalışırken geçici hataları ele almasını, başarısız bir işlemi şeffaf bir şekilde yeniden deneyerek sağlayın. Bu, uygulamanın kararlılığını artırabilir.

### 4. Circuit Breaker Pattern

Bir uzaktan hizmet veya kaynağa bağlanırken, kurtarılması belirli bir süre alabilecek hataları yönetin. Bu, bir uygulamanın kararlılığını ve dayanıklılığını artırabilir.

### 5. API Composition Pattern

Bu desen, veriyi elinde bulunduran bireysel mikroservisleri çağırarak bir sorguyu gerçekleştirmek için bir API düzenleyici veya birleştirici kullanır. Daha sonra sonuçları bellek içi birleştirerek birleştirir.

## Depo Yapısı

Bu depo, yukarıda belirtilen her bir tasarım deseni için örnekler sunmak üzere yapılandırılmıştır. Her desen, çözüm içinde ayrı bir proje olarak uygulanmıştır.

