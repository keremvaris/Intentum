# Intent Layer: 5 Devrimsel Demo – Spesifikasyonlar

Bu belge, Blazor örnek projesinde uygulanan "Intent Layer" beş demo senaryosunun (Project Pulse, Customer Journey, Moderation, Adaptive Tutor, Digital Twin) status tablolarını, event kataloglarını, varyant eşlemesini, politika kurallarını ve API özetini tanımlar. Her demo "Gözlemle → Tahmin Et → Karar Ver" üçlüsünü sektöre özgü niyet katmanı ile gösterir.

---

## Genel Bakış

| Senaryo | Status / aggregate alanları | Varyantlar (A/B/C/D) | Çıkarılan intent örnekleri |
|---------|-----------------------------|----------------------|----------------------------|
| 1 – Project Pulse | SprintStatus, TaskStatus, UserRole, TeamPulseStatus | A=BurnoutRisk, B=Healthy, C=ScopeCreep, D=DependencyBlocked | TechnicalDebtCrisisAndTeamBurnoutImminent, TeamOnTrack, FeatureScopeBeginningToCreep, CriticalDependencyBeingNeglected |
| 2 – Customer Journey | JourneyStage, IntentSegment, ReferralSource | A=Technical, B=PriceSensitive, C=Academic, D=Abandoning | TechnicalDecisionMaker_ComparingEnterprise, ComparingPricesAggressively, ResearchingForAcademicPurpose, OnTheVergeOfAbandoningBrand |
| 3 – Moderation | UserModerationStatus, ThreadRole, ToneScore | A=Trolling, B=PersonalAttack, C=SeekingHelp, D=Constructive | DeliberateProvocation_DerailingTechnicalDiscussion, VeeringOffTopicIntoPersonalAttack, GenuinelySeekingHelpButFrustrated, ConstructiveTechnicalDebate |
| 4 – Adaptive Tutor | LearningProgressStatus, StruggleType | A=ConceptualBlock, B=SurfaceLevel, C=LosingMotivation, D=Ready | ConceptualBlock_NeedsAlternativeExplanation, SurfaceLevelUnderstanding_SeekingQuickAnswer, LosingMotivationDueToPace, ReadyForNextModule |
| 5 – Digital Twin | ComponentHealthStatus, SystemOrientation, ExternalInputStatus | A=SystemicBottleneck, B=CostOverSpeed, C=Stable, D=SinglePointOfFailure | ConvergingTowardSystemicBottleneckAndMissedSLAs, OptimizingForCostOverSpeed, StableWithinSLA, SinglePointOfFailure_Emerging |

---

## Demo 1: Project Pulse (The Pulse)

**Konsept:** Takım olaylarından (TaskCompleted gecikmeli, PR gece, Estimate artışı, mesaj tonu) niyet çıkarımı; politika ile Warn, RequireAuth, Block (takvim bloğu).

**Status tabloları:** SprintStatus (OnTrack, AtRisk, Overdue, Cancelled), TaskStatus (Todo, InProgress, Blocked, Done, DoneLate), UserRole (Dev, QA, PM, Designer, DevOps), TeamPulseStatus (Healthy, Stressed, BurnoutRisk, ScopeCreep, DependencyBlocked).

**Event kataloğu:** TaskCompleted (DaysLate, SprintId), TaskBlocked (BlockerRole, BlockedSinceHours), PR_Created (HourOfDay, IsHotfix), Message_Sent (SentimentScore, Channel), Estimate_Increased (OldPoints, NewPoints), Meeting_Attended (DurationMinutes), Leave_Requested, Deployment_Failed (Environment, RollbackDone).

**Varyant eşlemesi:** A=4 olay (Burnout) → Warn + RequireAuth + Block; B=3 olay (Healthy) → Allow; C=4 olay (ScopeCreep) → Warn + RequireAuth; D=3 olay (DependencyBlocked) → Escalate.

**Politika:** Block (BurnoutRisk yüksek), Warn (BurnoutRisk / TechnicalDebt), RequireAuth (ScopeCreep), Escalate (DependencyBlocked), Allow (TeamOnTrack).

**API:** POST /api/project-pulse/start (body: variant), POST /api/project-pulse/stop, GET /api/project-pulse/status, GET /api/project-pulse/stream (SSE).

**Wow anı:** Takım pulse gauge ve takvim blokları mock; yeni özellik kartları gri.

---

## Demo 2: Customer Journey (Chameleon Campaign)

**Konsept:** Müşteri dokunuş noktalarından niyet çıkarımı; içerik kişiselleştirme (teknik vs hobi).

**Status tabloları:** JourneyStage (Awareness, Consideration, Comparison, Decision, PostPurchase), IntentSegment, ReferralSource (Google_Ad, LinkedIn_Organic, Blog_Newsletter, Direct).

**Event kataloğu:** BlogPost_View (DurationSeconds, PostId), PricingPage_View (PlanHovered, DurationSeconds), PricingPage_ClickCompare (PlanA, PlanB), Cart_Add, Cart_Abandon (StepAbandoned), SupportChat_Open (PreviousTicketsCount), ReviewSection_Scroll, Video_Play (WatchPercent, Topic).

**Varyant eşlemesi:** A=Technical → Block popup, RequireAuth whitepaper/demo; B=PriceSensitive → Allow indirim popup; C=Academic → RequireAuth academic license; D=Abandoning → Escalate.

**Politika:** RequireAuth (TechnicalDemo, AcademicLicense), Escalate (Abandoning), Allow (Default).

**API:** POST /api/customer-journey/infer (body: variant).

**Wow anı:** Yan yana "Bu kullanıcıya gösterilen": teknik tablo + demo formu vs indirim popup.

---

## Demo 3: Moderation (Context Guardian)

**Konsept:** Forum thread'inde mesaj dizisinden bağlamsal niyet (trolling, konudan sapma, yardım arayan); kişiselleştirilmiş uyarı.

**Status tabloları:** UserModerationStatus (New, Warned, Observed, Restricted, Banned), ThreadRole (OP, Participant, Moderator), ToneScore (-1..1).

**Event kataloğu:** Post_Create (ThreadId, WordCount, ContainsCode, ToneScore), Reply (TargetUserId, TargetMessageId, TimeSinceThreadStart), Upvote, Report_Submit, Edit, Message_Delete.

**Varyant eşlemesi:** A=Trolling → Warn + Observe 3 mesaj; B=PersonalAttack → Warn; C=SeekingHelp → Observe (farklı uyarı metni); D=Constructive → Allow.

**Politika:** Warn (Trolling, PersonalAttack), Observe (SeekingHelp), Allow (ConstructiveTechnicalDebate).

**API:** POST /api/moderation/infer (body: variant).

**Wow anı:** Thread zaman çizelgesi (mesajlar renk), uyarı balonu metni varyanta göre.

---

## Demo 4: Adaptive Tutor (EdTech)

**Konsept:** Öğrenci olaylarından öğrenme niyeti; Block (sonraki quiz), RequireAuth (ek modül), Warn (eğitmene).

**Status tabloları:** LearningProgressStatus (OnTrack, Struggling, AtRisk, NeedsIntervention), StruggleType (Syntax_Error, Logic_Flaw, Timeout, PartialAnswer).

**Event kataloğu:** Video_Play (ModuleId, LoopCount, WatchPercent), Quiz_Attempt (OutcomeScore, StrugglePattern), Code_Submit, Forum_Post (Topic), Resource_Open, Session_Idle (IdleMinutes), Module_Complete (QuizScore).

**Varyant eşlemesi:** A=ConceptualBlock → Block Quiz 4, RequireAuth "Görsel Akış Diyagramları", Warn eğitmene; B=SurfaceLevel → Warn; C=LosingMotivation → RequireAuth hız/mola; D=Ready → Allow.

**Politika:** Block (ConceptualBlock), RequireAuth (ExtraModule, hız ayarı), Warn (eğitmene), Allow (ReadyForNextModule).

**API:** POST /api/adaptive-tutor/infer (body: variant).

**Wow anı:** Yol haritası: Quiz 4 bloke (kırmızı), ek modül öne alındı.

---

## Demo 5: Digital Twin (Oracle of Operations)

**Konsept:** Depo/üretim bileşenlerinden sistemik niyet; Escalate, What-If önerisi (recommendedScenario).

**Status tabloları:** ComponentHealthStatus (Healthy, Degraded, Failing, Offline), SystemOrientation, ExternalInputStatus (None, WeatherAlert, DemandSpike, SupplierDelay, MaintenanceWindow).

**Event kataloğu:** Throughput_Report (Value, DeviationFromBaseline), ErrorRate_Report (Value, DeviationFromBaseline, ErrorCategory), EnergyConsumption_Report, InventoryLevel_Changed, ExternalInput_Received (Type, Severity), Maintenance_Completed.

**Varyant eşlemesi:** A=SystemicBottleneck → Escalate, What-If "Robot_2 devre dışı + yedek rotalar"; B=CostOverSpeed → Observe, Warn; C=Stable → Allow; D=SinglePointOfFailure → Escalate, What-If "Robot_2 bypass".

**Politika:** Escalate (Bottleneck, SinglePointOfFailure), Warn (OptimizingForCostOverSpeed), Allow (StableWithinSLA).

**API:** POST /api/digital-twin/infer (body: variant).

**Wow anı:** recommendedScenario metni (What-If); split ekran mevcut vs önerilen senaryo (UI'da metin olarak).

---

## Uygulama Referansı

- **Blazor sayfaları:** /project-pulse, /customer-journey, /moderation, /adaptive-tutor, /digital-twin.
- **Ortak bileşenler:** BehaviorEvent, BehaviorSpace, IIntentModel (RuleBasedIntentModel veya ChainedIntentModel), IntentPolicy, PolicyDecision.
- **Varyant verisi:** Her senaryo için Api/XxxVariants.cs (olay listeleri + beklenen intent); API body ile variant=A|B|C|D seçilir.

Bu spesifikasyonlar [case-studies README](README.md) ile birlikte Intent Layer 5 demo implementasyonu için referans olarak kullanılır.
