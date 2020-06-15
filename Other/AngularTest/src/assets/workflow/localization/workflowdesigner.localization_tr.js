﻿var AngularBPWorkflowDesignerConstants = {
  ActivityColor: "#ECF0F1",
  ActivityTextColor: "#2D3436",
  ActivityInitialColor: "#27AE60",
  ActivityInitialTextColor: "#FFFFFF",
  ActivityFinalColor: "#2980B9",
  ActivityFinalTextColor: "#FFFFFF",
  ActivityShape: "#CECECE",
  SelectColor: "#F39C12",
  SelectTextColor: "#FFFFFF",
  SelectSubProcessColor: "#e3b015",
  SelectSubProcessTextColor: "#FFFFFF",
  ButtonActive: "#D4D8D9",
  BarColor: "#EDF1F2",
  BarSeparatorColor: "#D9DEE0",
  DeleteConfirm: "Seçilen öğeleri silmek istediğinizden emin misiniz?",
  DeleteConfirmCurrent: "Bu öğeyi silmek istediğinizden emin misiniz?",
  FieldIsRequired: "Alan gereklidir!",
  FieldMustBeUnique: "Alan benzersiz olmalı!",
  ButtonTextDelete: "Silmek",
  ButtonTextCreate: "Yaratmak",
  ButtonTextSave: "Kayıt etmek",
  ButtonTextYes: "Evet",
  ButtonTextNo: "Yok hayır",
  ButtonTextCancel: "İptal etmek",
  ButtonTextClose: "Kapat",
  ButtonTextUndo: "Geri alma",
  ButtonTextRedo: "Yeniden yapmak",
  SaveConfirm: "Değişiklikleri Kaydet?",
  CloseWithoutSaving: "Kaydetmeden kapatılsın mı?",
  DialogConfirmText: "Soru",
  None: "Yok",
  Warning: "Uyarı",
  InfoBlockLabel: {
    Activity: "Etkinlikler:",
    Transition: "Geçişler:",
    Command: "Komutlar:"
  },
  ActivityNamePrefix: "Aktivite_",
  ActivityFormLabel: {
    Title: "Aktivite",
    Name: "Isim",
    State: "Belirtmek, bildirmek",
    IsInitial: "Ilk",
    IsFinal: "Final",
    IsForSetState: "Ayarlanmış durum için",
    IsAutoSchemeUpdate: "Otomatik şema güncellemesi",
    Implementation: "Uygulama",
    PreExecutionImplementation: "PreExecution Uygulaması",
    ImpOrder: "Sipariş",
    ImpAction: "Aksiyon",
    ImpActionParameter: "Eylem parametresi",
    AlwaysConditionShouldBeSingle: "Her zaman durum tek olmalı",
    OtherwiseConditionShouldBeSingle: "Aksi halde durum tek olmalı"
  },
  TransitionFormLabel: {
    Title: "Geçiş",
    Name: "Isim",
    From: "Faaliyetten",
    To: "Faaliyete",
    Classifier: "Sınıflandırıcıyı",
    Restrictions: "Kısıtlamalar",
    RestrictionsType: "Tip",
    RestrictionsActor: "Aktör",
    Condition: "Şart",
    ConditionType: "Tip",
    ConditionAction: "Aksiyon",
    ResultOnPreExecution: "PreExecution'da Sonuç",
    Trigger: "Tetik",
    TriggerType: "Tip",
    TriggerCommand: "Komuta",
    TriggerTimer: "Kronometre",
    ConditionActionParameter: "Eylem parametresi",
    ConditionInversion: "Eylem sonucunu tersine çevir",
    ConditionsConcatenationType: "Koşulları birleştirme türü",
    AllowConcatenationType: "Concat olarak izin ver",
    RestrictConcatenationType: "Concat kısıtlaması",
    ConditionsListShouldNotBeEmpty: "Koşullar listesi boş bırakılmamalıdır",
    IsFork: "Çatal mı",
    MergeViaSetState: "Ayarlanmış durum aracılığıyla alt süreci birleştir",
    DisableParentStateControl: "Ana işlem denetimini devre dışı bırak",
    ShowConcatParameters: "Birleştirme göster",
    HideConcatParameters: "Birleştirmeyi gizle"
  },
  LocalizationFormLabel: {
    Title: "Yerelleştirme",
    ObjectName: "Nesne adı",
    Type: "Tip",
    IsDefault: "IsDefault",
    Culture: "Kültür",
    Value: "Değer",
    Types: [
      "Command",
      "State",
      "Parameter"
    ]
  },
  TimerFormLabel: {
    Title: "Zamanlayıcılar",
    Name: "Isim",
    Type: "Tip",
    Value: "Değer",
    Types: [
      "Command",
      "State",
      "Parameter"
    ],
    NotOverrideIfExists: "Varsa zamanlayıcıyı geçersiz kılmayın"
  },
  ParameterFormLabel: {
    Title: "Parametreler",
    Name: "Isim",
    Type: "Tip",
    Purpose: "amaç",
    Value: "Değer",
    InitialValue: "Başlangıç ​​Değeri",
    ShowSystemParameters: "Sistem parametrelerini göster"
  },
  ActorFormLabel: {
    Title: "Aktörler",
    Name: "Isim",
    Rule: "Kural",
    Value: "Değer"
  },
  CommandFormLabel: {
    Title: "komuta",
    Name: "Isim",
    InputParameters: "Giriş parametreleri",
    InputParametersName: "Isim",
    InputParametersIsRequired: "gereklidir",
    InputParametersParameter: "Parametre",
    InputParametersDefaultValue: "Varsayılan"
  },
  AdditionalParamsFormLabel: {
    Title: "Ek Parametreler",
    IsObsolete: "Eski",
    DefiningParameters: "Parametreleri tanımlama",
    ProcessParameters: "İşlem parametreleri",
    ProcessParametersName: "Isim",
    ProcessParametersValue: "Değer"
  },
  CodeActionsFormLabel: {
    Title: "Kod eylemleri",
    Name: "Isim",
    ActionCode: "Eylem kodu",
    IsGlobal: "Küresel mi",
    IsAsync: "zaman uyumsuz",
    Type: "Tip",
    GlobalDeleteMessage: "Global CodeAction'ı sildiniz. <br/> <b> Diğer şemalar bu CodeAction'ı çağıramayacak! </ B>",
    UnGlobalMessage: "Küresel bayrağın durumunu değiştirdiniz. <br/> Bu şemayı kaydettikten sonra bu Global CodeAction'a dayalı bir Yerel Kodlama Oluşturulacak."
  },
  ToolbarLabel: {
    CreateActivity: "Etkinlik oluştur",
    CopySelected: "Seçilenleri kopyala",
    Undo: "Geri alma",
    Redo: "yeniden yapmak",
    Move: "Hareket",
    ZoomIn: "Yakınlaştır",
    ZoomOut: "Uzaklaştırmak",
    ZoomPositionDefault: "Yakınlaştırma varsayılanı",
    FullScreen: "Tam ekran",
    Refresh: "Yenile",
    AutoArrangement: "Otomatik düzenleme",
    Actors: "Aktörler",
    Commands: "Komutları",
    Parameters: "Parametreler",
    Localization: "Yerelleştirme",
    Timers: "Zamanlayıcılar",
    AdditionalParameters: "Ek Parametreler",
    CodeActions: "Kod eylemleri",
    Info: "Genişletilmiş bilgi",
    Delete: "Silmek",
    Clone: "Klon",
    Settings: "Ayarlar",
    CreateTransition: "Geçiş oluştur",
    CreateActivityTransition: "Bir etkinlik ve geçiş oluşturun",
    Legend: "Efsane"
  },
  ErrorActivityIsInitialCountText: "Bir öğe bayrakla işaretlenmiş olmalıdır",
  ErrorReadOnlySaveText: "ReadOnly modunda Tasarımcı, kaydedemezsiniz.",
  FormMaxHeight: 700,
  EditCodeSettings: {
    Height: 600,
    Width: 1000,
    CodeHeight: 390,
    MessageBoxHeight: 400,
    MessageBoxWidth: 600,
    SuccessBoxHeight: 150,
    SuccessBoxWidth: 300
  },
  EditCodeLabel: {
    Title: "Kodu düzenle",
    EditCodeButton: "Kodu düzenle",
    Usings: "Usings",
    Compile: "Derlemek",
    CompileSucceeded: "Derleme başarılı oldu.",
    Success: "Başarı",
    Error: "Hata",
    OK: "Tamam",
    ShowUsings: "Gösterileri göster",
    HideUsings: "Kullanımları gizle"
  },
  EditJSONSettings: {
    Height: 600,
    Width: 1000,
    CodeHeight: 480
  },
  EditJSONLabel: {
    Title: "JSON'daki Değeri düzenle",
    CreateEmptyType: "Yaratmak",
    Format: "Biçim"
  },
  isjava: false,
  OverviewMap: {
    show: true,
    width: 300,
    height: 150
  },
  UndoDepth: 200,
  DefaultCulture: "tr-TR"
};

