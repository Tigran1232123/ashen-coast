# V55 — синхронизация добычи

Готовое исправление: CoopWorldV49.Capture исключает collected loot; ApplyLoot обрабатывает collected как tombstone до Restore(), не создавая устаревший item. Нулевой count воспроизводится настоящим слиянием монет; в старом пути LanItemV49.Restore выбрасывает Invalid network item. При сбоях CoopDiagnosticsV55 записывает полное исключение и тип пакета в Coop-Sync-Error.txt возле exe или persistentDataPath, кнопка в CoopUI открывает его. Точная причина на ПК друга не подтверждена без журнала. Протокол/hello/app/меню55. Новые focused ChecksSyncV55 и два процесса LAN: поздний вход, повторные снимки, tombstone, reconnect. Старые тесты не повторять без затронутых изменений. Не закрывать пользовательскую игру.

Сборка work/Windows-V55, копия outputs/Windows-V55; запуск outputs/Запустить игру.cmd. GitHub разрешён, релиз v55. Документация CHECKS-V55, CHANGELOG-V55, COOP-V55. Ниже — история предыдущих версий.

## V54: Крылатый стан — 15 октября 2026

Пользователь возобновил публикации прямой просьбой «как доделаешь выгрузи на гитхаб». Порядок AGENTS снова действует. Подготовлена V54 (bundle54.0, packet/hello54), Windows/Unity ZIP и SHA256. V53 входит в V54; её отдельного публичного релиза не было.

Новые файлы VillagePolishV54/AnimationV54, WorldPolishV54, ArrowPhysicsV54. Village LayoutSeed случайный Guid hash, отдельный от WorldShape.Seed. Передаётся в welcome.villageSeed; ApplyLayoutSeed передвигает существующие дома, фундаменты и огни, перестраивает дороги и сохраняет ключи NPC/комнат/инвентарей. Poisson-подобное размещение, минимальное расстояние, огибание площади/клеток; дороги соединены через A* и дерево ближайших связей.

10 полных cutaway интерьеров AmazonHome0–9V54, родитель каждого — прежний rooms[house]. Передняя дверь exit находится(0,-4.65), вход(0,-3.4). Эллиптическая граница и три мебельных основания; горшки cell(2,-3), у вождя дополнительно(-3,-3), их спрайты подняты на bounds.extents.y до основания на полу. Общий очаг из одного AlvielHearthV54 с передней половиной отдельного слоя и28 языками огня. Огненный footprint7.9×3.5 блокирует прогулки по углям. 2 стойки учебного оружия, предметы с пола убраны.

AmazonCombatFrames4×8: тело целиком при атаке, ноги PixelGait только при ходьбе, отдельные кистевые опоры для копья/топора/кинжала/лука. Captives4×8 — разные мужчины, собственные полные ноги. SlaverLean6 — у первой клетки. Патруль делает паузу по достижении точки; тренировка чередует атакующих, отступление и шаги. LanRender.floor синхронизирует обрезку торса с отдельным слоем ног.

Стрелы: общая логика игрока/амазонок/равнинных/фортовых лучников, 36damage,2s. Ground plane+height, gravity9.8, speed13.5, траектория к дальности цели. Сегментные проверки шагом≤.025, ограничение64 стрел, остаются до18s. Клиентские стрелы двигаются вне guard SimulateWorld и посылают урон авторитету. LanDamage.delivery сохраняет Melee/Tentacle/Other; летящий дракон игнорирует первые два. GoldenSpear stun до Hurt; WoodlouseVitals.Hurt(preventCurl) не включает защиту при действующем стане. Старая свёрнутая защита остаётся.

Сапоги со старта; WearingBoots проверяется при надевании/движении роликов. BaseMovementSpeed содержит текущие множители, RollerMotion применяет1+Bonus один раз. FormationPointV54 хранит след каждого владельца и ведёт две колонки; NPC и маг используют тот же расчёт. ActorVital(always) для союзников; маг с маной. TooltipV54 рисуется последним рядом с Pointer, отсутствует при кнопках/переносе/паузе; старый DrawDetail удалён. Червь3×16 новыхкадров с фиксированным масштабом, повороты и перистальтика. Zenangle60, Atmosphere — округлённое смещение резких пикселей от движущихся радиальных волн.

Проверки в CHECKS-V54, арт и промпты ART-V54, изменения CHANGELOG-V54. Новые рабочие edit_*.py одноразовые; НЕ запускать повторно. Сборка unity-build-v54-ready.log. Упаковка package_v54.py одноразовая. Предыдущие версии/проверки ниже — исторические. Не повторять нерелевантные тесты, не закрывать пользовательскую игру.

## V53: локальная доработка — 15 октября 2026

ПУБЛИКАЦИИ НА GITHUB ПРИОСТАНОВЛЕНЫ ПО ПРЯМОЙ ПРОСЬБЕ ПОЛЬЗОВАТЕЛЯ. Не push и не release без новой просьбы. Готовая локальная V53: work/Windows-V53 и outputs/Windows-V53. bundleVersion 53.0, LAN packet/hello53. Архивы Windows/Unity V53 и V53-SHA256.json. История опубликованной V52 сохранена.

Изменения и проверки: CHANGELOG-V53.md, CHECKS-V53.md, ART-V53.md. WorldPolishV53: ограниченный пул из трёх слышимых голосов NPC, координаты локального слушателя даже при временном coop Context, outdoor geometry для всех улиток и heap A*. SnailHomes/CaravanCamp используют отдельные пути до общей точки, затем до мест в кольце; минутный сбор остаётся. SnailPresentation сохраняет массив вершин и веса, новые комнаты/мебель V53 с _ArtStep1, дверями по измеренным координатам и твёрдыми основаниями мебели. Гарнизон стоит в центре и сбрасывает ходьбу при отдыхе.

PixelGait.ForHumanoidV53/PoseSideV53 добавлены амазонкам, пленным, равнинным/фортовым разбойникам, повозщику и трактирным жителям. Ноги берут авторские парные контактные/проходящие позы, фаза от расстояния. Новые персонажи тоже должны получать полный шаг, а не тряску корпуса (правило в AGENTS).

CorpseWorldV51 кэширует bindings раз в .5s и проверяет изменение жив/мёртв раз в .05s; убраны выборки тел для каждого живого существа и постоянный RegionOf. Падение .7s из последнего живого спрайта, затем тело до60s. Размеры woodlouse2.2/small1.056, frogNPC1.15, playerfrog1.11. Нейтральным сбрасываются агрессор/Hostile/PlayerStruck и старые цели союзников; природные враги не становятся союзниками. corpseDrop от хоста идёт симулятору чужой области, с защитой от повторов. Старые dragon/hunter deaths исключены и сохранены. Имеющийся loot выбрасывается по дуге, для остальных существ небольшой drop создаётся один раз; инвентарь игроков сохранён.

Лягушка: герой1.068 вместо1.78, NPC около1.0, уменьшены руки/тень/коллизия. HandAnchor учитывает зелёные руки, SpectrumRaise отдельная узкая вырезка рукава, без торса. RemotePlayer.Receive устанавливает правильную расу даже до первого показа — это важно для трупа в чужой области.

Проверки: 28/0 в Verification-V53-map, финальные уточнения17/0, отдельный proxy LAN10/0, вырезка руки3/0. Независимые области, добыча/воскрешение и караван при ведущем внутри прошли двумя процессами; ошибка скрытого прокси из раннего прогона исправлена и проверена отдельно. Старые ModelChecks/полные LAN сценарии не повторялись. CPU100сканов:34.37ms→.39ms; Mono allocation counter возвращает0, память/FPS количественно не заявлять. Физические ПК/Radmin не проверялись. Пользовательские игры не закрывались. Сборка unity-build-v53-local-final.log.

Локальная git-ветка codex/local-polish-v53; ничего не отправлять на GitHub. Новый запрос после V53 — большая доработка Крылатого стана, интерфейса и боёв — выполняется следующим локальным обновлением V54.

## V52: исправление нарезки лягушки — 15 октября 2026

Текущая версия V52, bundleVersion52.0, LAN packet/hello52; всем участникам нужна V52. Windows: work/Windows-V52 и outputs/Windows-V52. Локальный запуск обновлён. Старые релизы сохранены.

Причина ошибки V51 на пользовательских скриншотах: FrogHeroV51 нарезался по равным колонкам256px, хотя кадры лежат с центрами примерно244/427/609/790px. Кадр0 обрезался справа,1 захватывал обломок0,2 захватывал начало3,3 показывал только правую часть. CharacterArtV51 теперь берёт фактические ColumnBands для каждой строки, с2px запаса. Bitmap не менялся, параметры и анимационная логика сохранены. ArtBank.ColumnBands стал публичным, его прежние вызовы/алгоритм не менялись. Существующие сетевые прокси используют исправленные кадры через HeroFramesV51.

Отдельно исправлен зеркально повёрнутый третий кадр западного профиля: FrogFrameMirroredV52 используется при ходьбе и в RemotePlayerV49. Другие кадры не отражаются.

Новый изолированный FrogChecksV52 (-frog-check -frog-output <folder>) проверяет32 физически целых силуэта по границам исходного листа,8 реальных направлений движения и8 поз сетевого прокси, а также идентификаторы версии. 49passed/0failed в work/Verification-V52-frog-final/results.txt. PNG frog-all-32-poses, frog-in-world и local-and-coop-frogs просмотрены; меню использует тот же целый кадр0/3. ScreenCapture скрытого окна дал чёрный character-menu.png, поэтому он не считается визуальным подтверждением меню. BuildCurrent: work/unity-build-v52-final.log. Старые игровые/сетевые сценарии не повторялись, поскольку изменено отображение лягушки. Работавшая пользовательская outputs/Windows-V51 не закрывалась. Два физических ПК/Radmin не проверялись.

Репозиторий: work/GitHub-Ashen-Coast, https://github.com/Tigran1232123/ashen-coast. Ветка codex/frog-sprites-v52; публикация отдельного v52 по AGENTS.md после загрузки и проверки черновика. Арт остаётся из V51, его промпты в ART-V51.md. Не запускать одноразовые скрипты переноса V51.

## V51: актуальное состояние — 15 октября 2026

Готовая Windows-сборка: Windows-V51. Исходники: outputs/AshenCoast. bundleVersion 51.0, LAN packet version и hello build 51. Всем игрокам нужна одна V51. Старые релизы сохранены. Локальный запуск: outputs/Запустить игру.cmd.

Новые модули: CharacterV51, SnailPresentationV51, CaravanCampV51, CorpseWorldV51, ReturnSpellV51, ChecksV51. Арт: пять PNG V51, описания и промпты в ART-V51.md. Использован встроенный image_gen.

CharacterV51: человек 100HP/100mana; лягушка 50HP/200mana. Выбор до начала, собственный полный атлас 8 направлений; человеческая нижняя часть отключена для лягушки. Race передаётся в player state, смена placeholder-прокси пересоздаёт походку. Выбор персонажа сразу обновляет texture property block. Main HUD использует MaxHealth/MaxMana.

SnailPresentationV51: V50 atlas нарезан по реальным границам (первая колонка 900 px), голова не обрезается. Подразделённый MeshRenderer с неизменными UV анимирует голову и подошву; SpriteRenderer остаётся для выбора/границ и forceRenderingOff. Нельзя использовать OverrideGeometry для этой деформации: он пересчитывает UV и не переносит текстуру вместе с головой. Втягивание .95s / выход 1.25s, корректный crossfade в закрытую раковину. Mathf.SmoothStep принимает from/to/t, поэтому пороги переводятся через InverseLerp. У каждого дома фон-раковина из отдельной клетки ShellInteriorsV51; back-door (.8,-.4), вход ниже на1.3; floor clamp. Кооперативная личная кровать дополнительно к двум трёхъярусным; rack/bow зависят от жильцов.

Camp phase:1 сбор60s,2 движение,3 расстановка,4 лагерь. Все живые нанятые войска возвращаются; через60s опоздавшие внутри. Укрытие любой улитки блокирует весь караван; в скрытую улитку войска не помещаются до выхода. Z также останавливает расстановку. Arrival четыре диагональные точки кольца, с FindLand у берега. Маг рядом с лагерем или в гарнизоне выходит и разжигает центральный огонь. Один отдыхающий на каждое бревно, отдых60s, бывший пленник иногда играет на флейте; четыре позы и мелодия. Camp state/route/troop poses идут глобальным caravanState, включая когда ведущий в комнате. Camp resting исключён из обычного follow и региональных snapshots. Command order проверяет локальное исследование карты; host принимает known из команды участника. Только его локальная карта задаёт известность точки, отдельного общего тумана войны нет.

CorpseWorldV51: хост отслеживает привязки существ и игроков во всех областях. Труп60s, fade последние6s, один на одну смерть; dragon и hunter (аксолотль) исключены. Неподвижные тела — 9 семейств спрайтов. Новый ReturnScroll в начальном инвентаре; SpellKind.Return, замкнутый полукруг (дуга+хорда), base30mana, огненная книга21, без cooldown. Ближайшее к центру тело внутри фигуры. Mana резервируется до host ack, при отказе возвращается. Возвращение NPC сохраняет Hired/hostile/assignment; игрока — race и private inventory. HP50%. Участнику приходит raisePlayer, симулятору чужой области raiseActor; двухсекундная защита от повторного создания тела по запоздавшему мёртвому snapshot. Невстроенные заклинания в Tome/Abyss по-прежнему заменяются ромбом/туманом. Место погибшего союзника удерживается, пока тело существует.

Lamp contact damage30 вместо10, hostile lamp24 вместо8. Dragon-fire12 и взрывы не менялись.

Проверки только V51: Verification-V51-final проверил сбор, реальное прибытие кольцом, комнаты, трупы, принадлежность союзника, символ, стоимость и усиление лампы;29 passed, единственное падение старого способа деформации исправлено заменой на mesh. Окончательная графическая проверка Verification-V51-art-ready:9 passed/0 failed, PNG всех комнат и лагеря просмотрены. Verification-V51-LAN-final: host14/client1=9/client2=10/client3=9, все0 failed;4 реальных процесса, frog max stats, смерть/воскрешение другого игрока с вещами, чужое воскрешение host NPC, gather/route/hide. Сетевая проверка была до последних правок только MeshRenderer/opacity; повторять её из-за визуальных правок не требовалось. Финальная сборка: work/unity-build-v51-ready3.log. ReturnRune дополнительно проверяет площадь и прямую хорду, чтобы не перехватывать круг/треугольник; отдельная Verification-V51-rune:9 passed/0 failed. Не запускались старые ModelChecks или RuntimeChecks. Radmin на двух физических ПК не проверен. Пользовательские игры не закрывались.

Репозиторий: work/GitHub-Ashen-Coast, https://github.com/Tigran1232123/ashen-coast. Не использовать унаследованный git root домашней папки или старый tigranisg-lab/5. AGENTS.md разрешает публикацию каждого готового обновления без повторного запроса. Сначала assets черновика + digest verification, затем publish v51.

## V50: актуальное состояние — 15 октября 2026

Готовая Windows-сборка: Windows-V50. Исходники: outputs/AshenCoast. Сетевой протокол и build greeting теперь 50. V49 и V50 в одной сессии несовместимы.

Добавлены SnailHomesV50, SnailUIV50, SnailActorsV50, GroundItemsV50. Четыре живые улитки-дома выше начальной повозки; уютные комнаты и казармы. В начале выбирается плановое число игроков 1–4. Владение: 1 -> все четыре; 2 -> по две; 3 -> одна каждому и общая; 4 -> одна каждому. Казарма 10 мест, mage 3, amazon 2, captive 1; personal solo snail 0 capacity, co-op personal snail 10 capacity. Stable seats peers[4] назначаются при приветствии; общий дом допускает воинов разных владельцев, команды остаются личными. При найме сначала выбирается дом; хост атомарно проверяет доступ и остаток вместимости перед оплатой. Hire packet carries key|home, receive не меняет исходный packet перед broadcast.

Snail state синхронизируется отдельным хостовым channel caravanState на всех областях: 1000 HP, regen 2/s, retraction 250 loss/5 seconds, hide 15 seconds. Occupied house защищён от retract и не умирает при находящемся внутри игроке. Глобальная колонна может двигаться даже когда ведущий в комнате; speed WalkSpeed*1.5. Карта command: LMB pan, RMB destination; all snails reveal map radius11. Z -> whistle lure2screens + global stop15s. Rooms persistent roots snail/0..3, WorldArea.SnailHouse, MapRegion.SnailHouse. При внешнем переходе из комнаты Inside очищается. Old enum casts adjusted for map spells. Дверь улитки -> вход, остальное тело -> карта. Кнопка призыва войск находится внутри комнаты.

TroopState holds home/seat/resting/returning. Garrison authority host regardless occupied region, heals 8HP/s + mage10mana/s; these actors excluded from regional snapshots during rest/return. Returning follows persistent village routes to leave fences; moving replica pose comes from global state. Resting/returning allies bypass auto-follow/area warp. Host accepts remote world snapshots for health of actors in other regions. Mage portrait LMB routes explosion to owner-region simulator; snail portrait sends following troops home. Captive weapon editor retained through new ally offer.

GroundItemsV50 centralizes final ground pivot/rotation/anchor and shadow size. All definitions inspected in a contact-sheet; special planted Spectrum and Zen preserved. Ground books retain designed aspect ratios. PNG atlas Furniture uses explicit rects because equal grid boundaries cut neighboring sprites.

Validation: work/Verification-V50-polish 76 passed/0 failed; includes focused snail ownership/capacity/heal/retract/occupancy/move/whistle and every ground ItemKind. Visual PNGs of barracks, personal room, all ground items inspected. work/Verification-V50-LAN verified four stable seats, hire ownership/payment once, independent rooms, guest movement and whistle; its first rest test timed out because it moved a replica while the authoritative ally remained hundreds of units away. Separate targeted garrison test (all four processes passed) places the authoritative ally correctly and covers forbidden foreign commands, return/healing/recall; final folder Verification-V50-garrison-final verifies changed return logic. These checks are V50-only; unchanged prior tests were not rerun. Native Radmin two-PC connection still not tested. Do not close user games: no saves.

BuildCurrent logs work/unity-build-v50-publish.log. No Prepare/full ModelChecks. Source and archive publishing: Tigran1232123/ashen-coast. Prior release v49 preserved. Work source repo is work/GitHub-Ashen-Coast, never the unrelated inherited home Git repo. See AGENTS.md for ongoing publication authorization.

# Контекст для продолжения «Берега обломков»

Состояние сохранено 15 октября 2026 года после выпуска V49. Это рабочая сводка предыдущего чата. Следующие сообщения пользователя определяют новые изменения и имеют приоритет над этой сводкой.

Постоянное указание пользователя от 15 октября 2026: после каждого готового обновления игры обновлять Tigran1232123/ashen-coast и публиковать новый релиз с Windows ZIP, исходным ZIP и SHA-256; сохранять старые релизы. Повторное подтверждение публикации не нужно. Обновлять локальный запуск и номер сборки для кооператива; у всех игроков должна быть одна версия. Подробный порядок закреплён в AGENTS.md.

## V49: текущее состояние — 15 октября 2026

Актуальная готовая Windows-сборка: Windows-V49. Исходники: outputs/AshenCoast. Кооператив LAN до четырёх игроков, отдельные переходы между областями, порт TCP27849; Esc открывает настройки с кнопкой «Кооператив». Для Radmin VPN используется адрес его адаптера. Локальный launcher предпочитает work/Windows-V49, потому что для этого пути пользователь уже разрешил входящие подключения брандмауэра; portable ZIP запускает собственную папку Windows-V49.

Дополнительно проверены четыре случая изоляции карманных измерений: их хранилища и вещи не попадают в мир или карман другого игрока.

Новые модули: CoopTransportV49, CoopStateV49, CoopSessionV49, CoopPlayersV49, CoopWorldV49, CoopInventoryV49, CoopFxV49, CoopIntegrationV49, CoopRemainingV49, CoopPersistentEffectsV49, CoopUIV49. TCP с длиной кадра и Deflate, сетевые DTO без циклов nullable Unity JSON. Состояние каждой занятой области рассчитывается одним игроком (минимальный peer ID); ведущий координирует общие хранилища, CAS revisions и выдачу вещей. Предмет имеет Item.id, отдельное падение — GroundLoot.netIdV49. Replica FX имеют только Renderer; их урон повторно не рассчитывается. Магия статусов, тумана, щупалец, взрывов костра и дракона маршрутизируется к расчётчику области. Мана и инвентарь персонажа остаются локальными; World Snapshot никогда не включает основной спрайт игрока в манекен. Сроки предметов и cooldown учитывают сетевое время.

Полезные проверки: LanNativeChecksV49 (-lan-native-check), LanRemainingChecksV49 (-lan-remaining-check вместе с native), LanHireChecksV49 (-lan-hire-check). Они запускают четыре отдельных процесса. Первая серия подтвердила независимые области и передачу здоровья; повторный подбор исправлен отдельной серией. Окончательная серия проверяет только затронутые сетевые сценарии, без повторения сотен прежних моделей/UI/книг. Логи находятся в work/Verification-V49-* и work/unity-build-v49-release.log. Первые незавершённые отчёты исторические, окончательный final — для релиза. Скрытое нативное окно дало чёрный скриншот, поэтому он не используется как подтверждение визуальной проверки.

Ограничения: соединение между двумя физическими ПК и Radmin пока не проверено; сохранений мира и восстановления персонажа после переподключения пока нет; выход ведущего завершает сессию. Пользовательские игровые процессы не закрывать: сохранений нет. Будущие правки проверять только по затронутым сценариям.

Публикация: GitHub Tigran1232123/ashen-coast, отдельный репозиторий по явному выбору пользователя. Старый tigranisg-lab/5 содержит другие Python-проекты и не менялся. Исходники GitHub в AshenCoast, Windows и исходный ZIP — в Release v49.

## V48: предыдущее продолжение

Latest playable build: Windows-V48. Village and mannequin DPS improvements only. Initial focused native checks passed23 mechanics; the single failed interior count fixture counted15 despite14 actual intended props. Final four focused art/layout/DPS checks passed after correcting that fixture and fixing the interior arrangement. Old village hire/tree/stack and book/rainbow suites were not repeated. One1440x900 window; preserve user game processes. BuildCurrent, new versioned build directory and launcher.

AmazonLifeV48 adds upright segmented side fences with connected wood rails, tight12-fire pedestal ring, cobbled spokes from all ten houses and a circular road. AmazonFurnishingsV48 alpha-preserved built-in imagegen atlas has nine tribal props; prompt/provenance in ART-V48.md. Chief hall has fourteen authored prop instances including four straw wall segments and four beds. Skin rug behind hearth, pots beside mats. AmazonArtV46 now uses one fixed PPU per woman across all walking frames.

VillageMovementV48 replaces instantaneous left/right steering with persistent A* waypoints, velocity smoothing and soft separation. Guards traverse village routes; three additional hunters roam around the outskirts and hunt woodlice/hostile enemies. Eight or ten randomized unhired guards start safe paired animated practice at phase120 (one minute before phase180 night), stop at225, and abort on village alarm. Practice arrows and melee are harmless. All valkyrie weapons are shown only while Attacking, including allies/training.

Weapon merchant at village east sells Dagger3,Stake4,Spear5,Axe6,Sword8 via Survivor.MerchantPriceV48 and the existing purchase flow. Existing two hireable meat hunters remain. PlainsLifeV48 rolls rare encounters only in newly explored48-unit chunks far from village/spawn/shore, offscreen29–39units, at mostfive live bandits; real archer/sword attacks, loot and shared targeting/HP/spells. Guards and hunting Amazons fight bandits without turning hostile to player.

DummyDpsV48 tracks actual player damage over a rolling3-second window, initial denominator minimum1second, fades after3seconds since last hit. DamageDummy player=false excluded. PracticeTarget damage through HurtTarget(source=hero) included. Drawn above shore and village mannequins.

NEXT REQUIRED WORK: user explicitly prioritizes village first, then LAN co-op up to four players with independent location transitions. Co-op is not implemented in V48. Preliminary architecture in work/coop-v48-plan.md. Continue from outputs/AshenCoast, preserve V48 release before network changes; use V49 for co-op. Never replace independent transitions with group travel. Latest added user requests included hiding valkyrie weapons outside attacks and displaying player DPS over mannequins; these are complete in V48.

## V47: актуальное продолжение

Latest playable build: Windows-V47. Only the Coast map village marker changed. ExplorationMap.Landmark now receives always=true for AmazonSettlementV46.Centre, so the icon/name appear before visiting the settlement, clamp to the corresponding viewport edge when offscreen, and show at all zooms. The house glyph is drawn from native UI pixels rather than depending on a font glyph. Actual village position remains driven by the V46 settlement layout. Terrain exploration is unchanged. BuildCurrent completed successfully; no old gameplay checks were rerun for this small UI change, as requested by the user. Preserve existing user game processes. Launcher points to a separate Windows-V47 directory.

Continue from outputs/AshenCoast. All V46 mechanics and its focused check reports remain the baseline. Run only checks relevant to later changes.

## V46: актуальное продолжение

Latest playable build: Windows-V46. The initial36 focused native checks passed at1440x900. After correcting new atlas boundaries and interiors,10 changed checks passed, including real axe/tree contact and new party travel. The final dirt/camera correction then passed only 3 visual checks with updated native screenshots. Unchanged hire/trade/stack coverage and complete regressions/model suite were not repeated, per user request. Preserve all user game processes because there are no saves. Build uses ProjectBuilder.BuildCurrent; native launcher points to a separate Windows-V46 directory. Artwork provenance and exact prompts in ART-V46.md; original synthesized38.4-second drum/flute/horn score in Resources/Audio/AmazonBattleV46.wav.

AmazonSettlementV46 appears around(-135,230) on Coast plains, chooses another safe site if the procedurally located fort is too close. Centre is available via game.amazons.Centre; WorldShape.AmazonFootprint excludes procedural plants from settlement. Ten enterable cone houses, one chief hall, persistent stores, daytime/night flames, Alviel statue matching supplied sword pose, fenced targets and loose collectible axe/stake items. Opening doors switches WorldArea.AmazonHouse(5), mapped explicitly to MapRegion.AmazonHouse(6); Pocket remains map index5. ActiveRoot, world lighting, floor, interaction, movement and map support interiors. Source defines dwarf-free adult human captives with100HP, Amazon women150HP and four visual variants.

Two hunters expose hire/trade menu. UI VendorInventory selects the current hunter stock or original tavern seller and uses the existing paid transfer mechanism. RawMeat added to allowed merchant goods for1coin. All hire prices1coin per user; four captives release from bone cages and follow unarmed until player provides melee weapon. Ally weaponSlot is2x4 and has new optional acceptsItem/singleItem model constraints; ordinary grids retain their prior behavior. Armed allies fight hostile enemies; allied Amazons retain fixed innate weapons and never turn on player in a village alarm. Nearby un-hired Amazons retaliate after player strikes a resident. Guards include faster melee and real bow projectiles. WorldCreatures includes hired actors; AdditionalTargets includes active village non-allies and training targets. Health feedback and spell health queries support villagers.

Append ItemKind54 Axe(1x3,30damage,1s,120degree arc),55 Stake(1x4),56 Bow(2x3); ArtBank icons57. Axe adds20percent outgoing damage against armored target classification, without bypassing incoming limits. Seven poplars near settlement require ten axe contacts, fall away, drop3..5Wood, stay grounded5s after1.1sfall then fade. ShiftRMB takes ceil(count/2) preserving item condition; when holding an item puts one in clicked cell, respecting stack/spatial restrictions and blocking unpaid stock. CtrlRMB still throws; ShiftLMB transfer remains. Native V46 report is authoritative; old V44/V45 expectations are historical. Do not rerun one-time work/prepare_v46.py or finish_sources_v46.py.

Continue from outputs/AshenCoast. After changes, run only affected checks; do not repeat unchanged UI/book/rainbow suites or multiple resolutions without a concrete reason. Packaging updates source mirror, native/source archives, launcher and this context after the final native report agrees with the built DLL.

## V45: актуальное продолжение

Latest playable build: Windows-V45. Focused native checks: 16 at 1440x900, only changed spear/Zen mechanics and visuals. Unchanged inventory/book/rainbow, V41/V39 regressions and model checks were not repeated, as requested by the user. An earlier V45 build already passed 308 model assertions; final BuildCurrent report explicitly records their omission. Screenshots/reports in outputs/Verification with v45 prefixes. Exact built-in imagegen prompts/provenance and final spear path in ART-V45.md. Never close user game processes: there are no saves. Existing user V44 process was preserved; launcher starts a separate Windows-V45 directory.

Golden Sting loads SpearV45, using the reference's complete S-shaped shaft and two large open curls, no straight central pole or extra lower spike/collars. The image is aligned upright and wood colors matched to original reference. Natural uniform scaling in hand/floor, inventory icon area width38 within its1x4 footprint.30damage and one-second yellow stun unchanged. Alpha of the final built-in imagegen output is preserved. Discarded variants remain outside project.

Zen damage90 (previous60). Definition base attack duration.46/3 and cooldown.58/3. ZenIntervalV44 divides the old hunger curve by3 with a floor interval1/9; full hunger gives three times former speed, low/zero hunger caps at9APS. Duration/2-unit swept dash scale to the new cadence. Existing4hunger/missingHP cost, attack-only invulnerability, clockwise swing and repeat-heldLMB behavior retained. Source scheduling uses Time.time-start timestamp, with real-frame verification of rapid repeated attacks and between-attack vulnerability. Updated V44 checks expect current90damage/9APS and faster frame timing; old reports preserve previous values.

Ground Zen rotates+30degrees around its planted tip, which means30degrees farther left. Anchor lowered.16world units; ZenRopes clips submerged blade at the original floorY. Shared GroundPixelV45 applies the same clipping to hover and click selection. Rope animation/tip stability retained. CoastGame.GroundZenWellsV45 collects up to8 closest uncollected landed active Zens; PixelAtmosphere uploads centers/radii to Atmosphere shader. Local animated refraction radius1.45, displacement at most0.85 low-resolution scene pixels; it operates before UI and removes instantly on collection. No ground ripple during flight or in inactive worlds. The effect remains visible with the hero nearby.

Continue from outputs/AshenCoast. Authoritative current checks: -v45-checks-only runs only new reference/cadence/damage/ground-refraction checks, without V44 coverage or glyph layout. GPU checks verify localized moving scene pixels and no outside-radius effect, including when the hero stands near Zen. Use ProjectBuilder.BuildCurrent for prepared-project changes to avoid repeated unchanged checks/reimports; Build remains available for a fresh project. Do not repeat unrelated regression checks unless changes or failures justify them. Do not rerun work/prepare_v45.py. Packaging script updates source mirror, source/native archives, launcher and recovery context only after final build and reports agree.

## V44: актуальное продолжение

Latest playable build: Windows-V44. Native focused checks at three actual window resolutions: [85, 85, 85]; V41 regressions: 67; V39 regressions: 34. Model assertions in build-verification.txt. Final authored PNGs and exact built-in imagegen prompts/provenance in ART-V44.md. Native screenshots/reports under outputs/Verification with v44 prefixes. Never close user game processes: there are no saves. Updated launcher starts the separate Windows-V44 directory.

Appended enum IDs51 Rot,52 GoldenSpear,53 Zen; art icons54. New weapons begin in player inventory, packed before smaller starting items. Golden Sting30damage1x4, separate one-second stun with yellow GPU particles, no shackles mana-lock/cap-bypass. CombatStatusV44 applies whole-worm immobilization, freezes all AI actions and retains hit particles on lethal contacts. Bound remains separate from ImmobilizedV44.

Zen60damage2x4, ordinary scythe base cooldown.58/reach2; interval interpolates to1/3.5 at0hunger. Per attack consumes4hunger, missing part fromHP; refuses unaffordable strike. Actual attack duration scales with cooldown. Invulnerable only during active Zen attack; incoming damage returns between attacks. HeldLMB repeats Zen. Each attack dashes2units over at most.14seconds via small swept CanLand/WorldBlocked checks. Both scythes rotate clockwise; ordinary spear thickness0.83. New weapons retain native vertical artwork proportions, old melee icons still rotate for UI. Planted Zen uses dedicated inverted pose anchored to the far-left blade-tip pixel with the buried lower edge clipped; ZenRopes shader isolates animated hanging rope/pendant pixels while the blade stays fixed. Divine UI cells and uniformly animated name color apply to Spectrum and Zen.

InventoryUI Shift transfers fall back from full main inventory to spatial only with open tome; merchant purchases support that destination too. Craft optional fallback checks output space before consuming resources; main-inventory UI enables it only with open tome. CtrlRMB handled before book/spell/equip interactions; throws exact original item/stack and closes books on drop. Covered lower items and unpaid merchant items cannot be taken. Full/closed layers preserve sources. Auto pickup remains main-only as previously requested.

CookingWaterV43 Pour unchanged quantity; Take withdraws one half, maxbottle2. RMB held bottle on vessel pours; RMB empty bottle or partial bottle on visible water withdraws. LMB no longer pours. InventoryMagicV44 draws muted pixel water, ripples/round boiling bubbles, and dark animated abyss vortex. RenderTexture.active is preserved around each GUI effect draw so interface stays visible. UI instructions and success notifications removed; GeneralErrorV44 keeps generalized errors. Names, stats, buttons and costs preserved.

TickFoodV43 skips Brain; expires food by changing same Item to Rot1x1, preserving identity/count and refreshing ground sprite/scale. Fried food remains360seconds, raw/prepared720; frying refreshes fully. Brain stem artwork FoodV44. Kundali yo-yo combat height.492. Orb ground width.52 and inventory area scale.60. Bottle icon centered+3px. FireBookV44 has frontal perspective and natural authored aspect. Long ground weapon scaling uses consistent actual physical length/height for food, flowers, scrolls, containers, trinkets and weapons; Zen held/planted height2.35 with grip at31 percent of the staff, containers use their physical world size.

Rainbow departure hides former wall and preserves only ground cloud/spray fade; one visible moving curtain. Arrival clouds remain. U toggles night to next daylight or day to night without changing old StartNight method. Existing book shortRMB/open and frame-only spell slots, V42 opacity/covered-item rules, maps and other mechanics retained.

Continue from outputs/AshenCoast. Do not rerun one-time work/integrate_v44*.py or finalize_v44_sources.py. Authoritative checks: -v44-checks-only; prior -v43-checks-only retains superseded freshness, water controls and artwork expectations. V41 regression fixture updated for V44 fire cover; merchant model fixture clears unrelated starting equipment before checking fixed buy coordinates. These are fixture changes, not weakened assertions.

## V43: актуальное продолжение

Latest playable build: Windows-V43. Native focused checks at three actual window resolutions: [72, 72, 72]; V41 regressions: 67; V39 regressions: 34. Model assertions in build-verification.txt. Reports and native screenshots in outputs/Verification with v43 prefixes. Final authored PNGs and exact built-in imagegen prompts/provenance in ART-V43.md. Never close user game processes; there are no saves. Updated launcher starts the separate Windows-V43 directory.

CoastChangesV43 loads the new FireBookV43/AbyssBookV43 atlases and OpenFolioV43 ancient-script pages. Original spatial cover remains TomesV5. Closed spatial/abyss aspect0.84; fire0.60 with newly authored natural narrow perspective. Compact equipment rectangles94x106, main gridY370; gear icons fit their frames. Uniform animated Spectrum hover-name hue matches inventory behavior, single one-pixel shadow; subdued cubic golden edges with maxalpha0.24. SpectrumRaiseV43 rotates the original player arm about its shoulder, masks the resting arm and attaches weapon to the animated hand; late execution order prevents idle pose from overriding it. Ground rope sway is stronger, planted tip position unchanged. Rainbow contact damage300, other costs/durations unchanged.

Worm perception32, memory14, chase3.7, lunge5.8/cooldown1.7 with longer range and faster turning/repath; tiny motion epsilon prevents high-FPS initial lunge cancellation. Cave floor webs leave40 percent speed and do not lock feet; actual web-bolt hit retains two-second foot lock and draws procedural Cocoon shader. Gulls scale0.60 and never begin a return landing when player is within8 of landing approach. Campfire frames are isolated per row without negative/overlapping atlas bounds; installed pots/cauldrons rise0.29 above cell center and flame ceiling clips under the vessel.

Appended ItemKind IDs44..50: Cauldron, WaterBottle, Buttercup, Poppy, Bellflower, Clover, FourLeafClover; no old IDs changed. Cauldron inventory3x3, contents4x4, one world cell, only crafted on Anvil with 4x3 ingot pattern anchors(0,0),(2,0),(0,1),(2,1),(1,2). Each ingot2x1. Anvil craftingEnabled true and containerKind Anvil limits recipes to cauldron. Tables/anvils still eject contents when closed; both vessel types retain contents. Shared cooking output-space check uses actual vessel dimensions.

CookingWaterV43 stores waterRows on GridInventory, bottle water2 -> each LMB pour consumes1 and raises one row, keeps bottle in hand. Water is visual background with waves/boiling, never blocks placement. Supports placed or carried ordinary pots and heated vessels; no refilling or other water effect yet. Initial Survivor includes one full bottle. RMB installed-vessel interaction takes priority over collecting hidden loot; loose vessel RMB opens its own original contents without collection. RMB spell clicks preserve slots; LMB extracts removable scroll. Abyss locked spell also preserves original RMB drag-to-cell action (pending drag only, no short-click activation). Existing book short-RMB toggles and frame-only slot popup preserved.

Food freshness/shelfLife on Item: default720 game seconds (two full day-night cycles); fried/assortment360, frying refreshes whole duration even for old raw meat. Spoiled food cannot be eaten, remains in inventory. TickFoodV43 ages all reachable items once with HashSet through holding, bags, spatial layer, merchant, ship, stations and ground. Stack merging takes minimum freshness. Food/water gauges use DrawCondition in all grids/hand. No added durability for containers or sack.

Original and streamed meadow now support six flower species plus three-leaf clover; collect converts to FourLeafClover only when random roll<0.10. Clover has no additional effect yet. Golden-field clovers share current-root harvest checks; F collection ignores inactive-world items/plants, and coast burn does not destroy field plants. Infinity travel map now uses heldLMB panning and RMB confirmation, preserving40mana/radius10screens/allies. V42 lower-layer dimming only when covered is preserved.

Continue from outputs/AshenCoast; detailed user controls in CHANGELOG-V43.md. Do not rerun work/integrate_v43*.py: one-time replacements already applied. Authoritative new checks are -v43-checks-only; V41 and V39 regression flag modes use current open-book artwork expectations. Historical V34/V36/V40 tests still contain superseded geometry/web/short-RMB assertions.

## V42: актуальное продолжение

Latest playable build: Windows-V42. 34 existing V39 native regression checks PASSED; model assertions in build-verification.txt. Native screenshots and report in outputs/Verification with v42 prefix. Single rendering-condition change: InventoryUI.DrawGrid dims first-layer item icons only when FirstLayerCovered(item) is true; SpatialLayerOpen no longer globally dims all main inventory icons. Any upper footprint intersecting a lower item dims that lower item; removing the upper item restores opacity automatically. The old covered-item access restriction, automatic overlay placement, blue cells and all V41 behaviour remain.

Continue from outputs/AshenCoast. New behaviour described in CHANGELOG-V42.md. Never close user game processes: no saves. Existing user V41 was preserved; updated launcher starts V42 from a separate directory. No new artwork or test-only rendering helpers were added for this small visual change.

## V41: актуальное продолжение

Latest playable build: Windows-V41. Native focused checks at three window resolutions: [67, 67, 67]; V39 regressions: 34. Model assertions in build-verification.txt. Reports in outputs/Verification/runtime-verification-v41*.txt. Exact built-in imagegen prompts and original paths in ART-V41.md. New FireBookV41/AbyssBookV41 PNG atlases preserve alpha and use ArtBank.Bands to split their two authored poses. Spatial folio artwork remains TomesV5/OpenTomeV6, only visual aspect changes.

User clarified the latest book controls: a short RMB click toggles the book itself ONLY inside player inventory; dragging a complete RMB frame and releasing opens spell slots in inventory or external storage. InventoryUI.V39 distinguishes clicks from frames before resizing books. Closed fire book displays at aspect0.50 in its1x2 footprint; closed spatial/abyss books display at identical aspect0.96 with78x78 icon areas and corresponding ground sizes. Open poses use the new clean atlases for fire/abyss, spatial art unchanged. InventoryUI.V35 passes display aspect to ArtBank.DrawIcon. Do not revert to the superseded V40 short-click rule.

CoastChangesV41: persistent AutoPickup setting (defaultoff), active only for landed ground loot within1.85, main inventory only. No flowers, sack fallback, equipment changes, or pickups while travelling/paused/dead. Full inventory leaves loot untouched and retries every0.15s. Sack Definition.Durable no longer includes Sack. Hover labels22px without frame; Spectrum letters use animated separate hues. Ground loot names have priority over underlying flowers, matching collection order. Spectrum edge color now muted(.87,.79,.47), alpha0.65 toward edges.

CombatFeedback stores WasHit history for HP bars after damage from any attacker; existing player attribution remains separate. FrogMage remembers a valid living attacker and clears aggression if it disappears, with no player fallback. FrogActor and CoastGame.HurtTarget preserve source attribution; area/lamp routes pass source; frog emitted hostile fire targets its actual victim and can damage NPCs. Player attacks still provoke retaliation. Reset clears frog hit history; hunter respawn clears hit history too.

SpectrumTravelV41 detaches the former wall/cloud/spray/reflection objects into a stationary departure effect at takeoff0.55s, animates the old tail for1.65s and cleans mist at2.5s. A separate current waterfall flies without cloud/spray; arrival shows cloud animation at its moving lower end. SpectrumPresentationV38 shares PoseImpactV41 for active and departure effects. Reflected ground light only appears when grounded. Departure lights remain at origin. ExplorationMap.ProcessMapInputV41 pans with heldLMB, selects rainbow destination with RMB; other map travel retains LMB selection. No arbitrary distance cap was added.

Never close user game processes: no saves. Continue from outputs/AshenCoast. New behaviour described in CHANGELOG-V41.md. Historical V40 check mode contains expectations for its superseded short-click behaviour; the authoritative current UI and travel checks are -v41-checks-only.

## V40: актуальное продолжение

Latest playable build: Windows-V40. Focused native UI checks at three actual window resolutions: [34, 34, 34]; V39 regressions: 34. Model assertions in build-verification.txt. Reports in outputs/Verification/runtime-verification-v40*.txt. User clarified that book spell slots open ONLY after RMB drag-frame selection and release, never after a short RMB click. A frame must contain the whole visible book footprint. It can start on a book or an empty cell and run in either direction. Cancel on closing inventory; covered lower-layer books are inaccessible. Ordinary crafting continues through the shared frame state.

InventoryUI.V39.cs uses StartGridSelection/UpdateSelectionEnd/FinishBookSelection and full cell containment. DrawGlyphLine uses ArtBank.RotateUI to compose rotation in the already scaled GUI coordinate space; Unity GUIUtility.RotateAroundPivot was mixing the pivot with interface scaling and scattering symbols outside the popup at nonunit scales. RuntimeChecks.V40.cs verifies real OnGUI book frame controls, storage and crafting, plus GPU capture of all nine glyphs in popup and HUD cell sizes: zero bright glyph pixels outside the designated cells, each glyph visible, enclosing GUI matrix preserved. V39/V37 checks updated to use frame selection. No new artwork needed.

Never close user game processes: no saves. Existing V39 process was preserved; the updated launcher starts V40 in a separate process. Continue from outputs/AshenCoast. Changes described in CHANGELOG-V40.md.

## V39: актуальное продолжение

Latest playable build: Windows-V39. 34 focused V39 native checks, 55 V37 regressions and 23 V38 regressions PASSED; model assertions in build-verification.txt. Reports in outputs/Verification/runtime-verification-v39*.txt. Exact built-in imagegen edit prompt and new dagger file in ART-V39.md. Never close user game processes: there are no saves.

SpellGlyphsV39.cs is the shared minimal line-icon and open-rune template registry. Separate fog, tentacle and zigzag recognition chooses the nearest template regardless of selected slot; fog no longer accepts the tentacle symbol. GestureShape compares circle against polygon templates, tolerates imperfect ovals, small gaps and reversed drawing. Fire zigzag casts campfire detonation with Orb/LastPrism via ArcaneCombat.TryExplodeFireGlyph, with the existing discounted 7 mana; lamp interaction retained. User specified an abyss whirlpool icon and fire zigzag. Abyss slot still marks an inventory cell by click.

InventoryUI.V39.cs: book popup commits on RMB release over the selected item, including other stores; upper items always win pointer selection; any upper footprint blocks taking the entire lower item. Drop placement automatically chooses upper layer when placing over occupied lower cells with open tome, otherwise lower. Closed tome preserves upper items and permits withdrawal, refuses reinsertion over lower items. No layer switch button. Larger spell cells and HUD icons, minimal glyphs, diamonds only for removable tome spells. Popup and transient notice placed above HUD to avoid overlap. Spectrum cell overlay linearly increases yellow alpha toward rectangle edge. CoastArtV37 stops overriding Tome icons17/18, restoring TomesV5/OpenTomeV6.

Instant diamond MoveTo leaves the camera follow origin intact (panCamera=true), and normal CoastCamera frame updates pan toward the destination. Rainbow contact uses ellipse half-axes3.1/1.2, alpha0.58 and brighter saturated strands; waterfall/flight animation preserved. SpectrumV39 replaces dagger artwork, SpectrumSurface.cginc animates bright spectral steel for held/planted sprites and SpectrumIcon render texture for inventory. Planted tip and rope anchors remain fixed. CoastChangesV39 supplies brighter central light plus six colored light positions for each nearby grounded dagger; animated rainbow reflection is hidden upon collection. Curse particles initialized tiny and transparent to avoid a first-frame white square.

Continue from outputs/AshenCoast. New behavior described in CHANGELOG-V39.md. All earlier mechanics retained except the explicitly superseded selection timing, icon styling and layer switching.

## V38: актуальное продолжение

Latest playable build: Windows-V38. 23 focused V38 native checks and 55 V37 regression checks PASSED; Unity model assertions in build-verification.txt. Reports in outputs/Verification/runtime-verification-v38.txt and runtime-verification-v38-regression.txt. Authored images and exact built-in imagegen prompts in ART-V38.md. Never close user game processes: there are no saves.

CoastArtV38 overrides Spectrum icon and registers one planted sprite to its blade tip; ropeRect isolates the rope. RainbowFlowV38 is a separate vertically scrolling texture; RainbowCloudsV38 has 12 authored cloud/foam frames. SpectrumPresentationV38 layers clouds, splash, 48 tiny airborne droplets, dithered ground light and five colored world lights. Rainbow.shader uses world-size texture coordinates, animated downward sampling, off-viewport adaptive height, falling leading edge and a falling trailing edge on dismissal. Texture is not squashed as it disappears; clouds remain for the final afterglow. New RainbowMist and RainbowGround shaders are included by ProjectBuilder. WorldSprite animates only rope UVs and retains restrained metal iridescence. GroundLoot keeps one planted sprite and pivot, flight turns continuously toward the fixed landing point, tiny grit particles are initialized at their final small scale. Held/ground light revised; light follows the dagger in flight. Mechanics and V37 content preserved.

Native visual review includes arrival, impact, two different flow poses, exit tail, afterglow, held/inventory and grounded dagger. Native pixel comparisons confirm real flowing strands and cloud motion; position checks confirm no blade movement while rope animates. CONTINUE from outputs/AshenCoast.

## V37: актуальное продолжение

55 focused native checks PASSED. Unity Windows x64 build and model checks passed; work/unity-build-v37.log, work/player-test-v37.log, outputs/Verification/runtime-verification-v37.txt. Authored art and exact imagegen prompts in ART-V37.md. Latest playable output Windows-V37. Never close user game processes: there are no saves.

BookSpells.cs: permanent slots, scroll object retention, book mana rounding, Survivor.spatialInventory. InventoryModel appends ShacklesScroll41, CurseScroll42, Spectrum43; old enum ids retained. InventoryUI.Books controls book popup, bottom spell bar, switching layers, abyss cell and station ejection. Only player inventory grids permit book opening; external placement and ground loot close books. Spellcraft handles glyphs, strongest-target whole-worm binding, curse, fog perception, translated removable spells/cooldowns and animated teleports. SpatialMagic retains legacy star portal; new glyph dispatch happens first. ExplorationMap.Travel marks bounded infinity target selection and unbounded rainbow selection. SpectrumWeapon controls hold, wall/contact damage, clouds, maintenance, exit tail and player-only flight. Hunter attack presentation uses AxolotlAttackV37; original gait restored outside attacks. TentacleField70damage and animated spider submersion; worm bars drawn independently after player strikes.

User clarified: shackles10sec, zero damage, mana lock only bound enemy, whole worm; fog10mana, tentacle damage doubled; second layer only main inventory; rainbow unbounded distance, player only. Full behavior and controls in CHANGELOG-V37.md. Continue from outputs/AshenCoast by subsequent user requests.

## V36: актуальное продолжение

28 focused native checks passed; model count in build-verification.txt. Latest native build Windows-V36; launchers updated in both output roots. Never close user game processes: there are no saves.

Ground Orb width .60 versus .85 before; held width .32 and inventory 2x2/sprite size unchanged. Linen pad retains its size. Held and ground orb use _Emission .36; CoastGame.AppendOrbLights emits low-strength blue point lighting for equipped visible orb and up to eight nearest active uncollected orb drops. The collector removes light with the ground item, and hidden roots do not emit.

NightHunter AttackDamage100, AttackInterval1, PlayerPriorityDistance3.5. HurtTarget dispatches attacker-aware damage to HurtFrom. Melee, yoyo, prism, tentacles, lamps, blasts, dragon melee and dragon fire all retain source attribution. Friendly lamp source is captured at emission, dragon source is explicit for breath and falling fire. Last valid living attacker is remembered; nearby player overrides temporarily. Dead/hidden/other-world targets are discarded, aggression resets outside field/night or when the hunter respawns. Lamp/light-zone avoidance is retained. Short attack checks physical field walls and has a cooldown; movement stops at contact distance rather than crossing the target.

Catacomb torch objects and light contribution removed. CaveDungeon.WebRadius2.55, sprite diameter5.1, circular shader matches footprint. SpiderWeb shader has denser, thicker pale strands with a low-light brightness floor. Web timer or standing in floor web makes Snared true and SlowMultiplier0. FeetLocked includes snare but MovementLocked does not: prism Free and general attacks stay usable. FixedUpdate clears momentum/roller buildup while FeetLocked; meditation cannot start immobilized. Web expires at10sec. Focused checks include native motion, melee/lamp/prism while webbed, light/ground-size and attacker-priority behaviour. Screenshots in Verification-V36.

## V35: актуальное продолжение

42 focused native checks passed; model count in build-verification.txt. Latest native build Windows-V35; stable launchers updated. Never close user game processes, there are no saves. Current requested changes and clarifications appear first in ПРОЧТИ-МЕНЯ.md; art/prompts ART-V35.md; non-game-design assessment TECHNICAL-NOTES-V35.md.

RollerSkates appended enum40, icons41. Survivor.skateSlot 2x2 onlyKindRollerSkates stores one item independently of equipment Feet. Feet Fits also recognizes rollers, special Equip/EquipOrDrop/AutoEquip paths preserve boots; ground pickups work. Default inventory adds one pair, durability false. InventoryUI.skatesOpen popup, yellow feet border/badge, LMB holding/gear swap/withdrawal; wrong kinds rejected by GridInventory.onlyKind and CanEnter. RollerMotion has3sec ramp to30%, acceleration3base, release brake1.25base, opposite direction resets buildup, zero actual velocity resets. FixedUpdate calls MotionVelocity before shared boundary/collision handling. Existing movement during item transfers preserved.

BattleYoyoV35 replaces icon20; attack height .328 vs .82 (40% remaining), inventory38x38 instead28x30, inventory footprint1. OrbClothV35 under Orb if grid is not main/holding/Sack contents. GridInventory.containerKind set for bags/pots. GroundLoot.cloth hidden in flight, visible after landing, removed on collect; dropped sphere also keeps animated Orb shader.

TryRecipe factors compact whole-resource selection from output space. Three logs select3x1or3x2 for table3x3. Anvil no longer needscraftLimit4 if smaller region touches all five anchor items; patterns remain unchanged. UI distinguishes no result space from wrong recipe and consumes no ingredients on failure.

PotCooking.Stir converts all selected raw/beetle pieces in place instantly on RMB MouseUp, cancels affected auto batch, no cooking timer/readiness for pieces. No frying on selection repaint; spoon stays, fryFlash/.7sec orange sparks, steam/wobble. Automatic assortment12sec still has progress; gauges only reflect complete dishes.

CoastArtV34.CampfireFrames now fixed40 canvases/pivot, single HearthFire SpriteRenderer blends adjacent atlas frames30fps; the PNG is unchanged. Ground patch uses noisy elliptical alpha, neutralized capped light and charcoal centre. New unique flame material disposed on explosion. Fog now outdoor coast AND golden field, initial10sec world anchored. SpectralWorm.SpeedFactor lerp1.8..1 for1..10parts applied to pursuit, lunge, turning and pose clock; both new split worms are faster.

## V34: актуальное продолжение

58 focused native checks passed. Latest binaries Windows-V34 in sibling outputs; both launchers updated. User V33 process was preserved. Main changes described in ПРОЧТИ-МЕНЯ.md and assets/exact imagegen prompts in ART-V34.md.

CoastExpansionV34.cs loads CampfireFlame40V34 (actual 40 frames, original request 48), GullsV34 adaptive row slicing (third row 7 mapped to 8 steps), TavernMotionV34 56 poses, AxolotlCorpseV34 8 and KitchenItemsV34. ArtBank icons40; RoastMeat37/RoastBeetle38/MeatAssortment39 appended. Pot4x4 inventory/world1; Ship39 items/7wood/14clay.

InventoryUI.holding6x6 owns the single carried item; LMB grid/gear removes source immediately. Same/different grid occupied swap puts previous item in hand. CloseContainers preserves paid carried items, returns reserved unpaid merchant food. Actual purchase uses holding as source. All attack sources tag PlayerStruck only for player-owned damage. ActorVital no names/15px numeric bars, hidden until player strikes; worm whole bar tracks any segment.

PotCooking.cs heated pot permits campfire overlap only, raised preview/renderer, shares fire collider; 25 slots persist. Manual selection <=4x4 consumes raw pieces after5sec (stirring1.7x); auto raw+beetle12sec => assortment. Cooking batch verifies ingredient presence and output space atomically. Wooden-spoon pointer and gauge/steam. CookingSteam shader.

CaveDungeon worm trail retains sample anchor instead of overwriting subthreshold movement. 16head/16trunk/16tail new images; heading limited155deg/s, smooth tangent rotations, route lookahead/lead/short4.8 lunge/patrol. Segment cap/splits preserved. Spider close bite12 once at.28sec with short lunge, web at.35sec retained. HunterPresentation fixed registered source body per view, view hysteresis, 144 smooth independent leg phases at80%; HunterDeath rest8poses/10sec/dissolve1.5, no active creature respawn until next night. Dark serif doubled point-filtered whispers.

TavernCombat alarm7residents: 5fighters (knight halfdamage/sword; others knives), 2workers flee. Neutral seated/gesture animations retained, new motion used only combat/flee. Gulls4state animated launch/flee atscreenheight/3/offviewport before hiding, userMP3 ambient/flight. RoosterImpactV34 synthWAV once landing. Rooster selection WorldCreatures nearest living, mouth/peck/claw/fire contact affect all mobs/player; single summon includespending/corpse. Whole-map coast fog initial10sec, world anchor.

## V33: катакомбы и доработки V32

59 focused runtime checks PASSED; model count is in build-verification.txt.

CaveDungeon.cs: CatacombLayout uses five organic rooms and connecting 3.3-unit tunnels, floors rasterized to world cells. Every drawn boundary wall has exactly one cell collider. CaveStone shader uses CaveTilesV33; entrance is seeded independently from fort and placed on safe coast land. WorldArea.Catacombs=4; MapRegion.Catacombs=4/Pocket=5; six explored regions. ActiveRoot, visibility, map, camera, interactions, ambient/torch lights, placement and ally pathfinding extended. CoastGame.WorldBlocked selects the active dungeon, shared combat checks use it; flame particles stop at stone walls.

SpiderV33: 32 frames, seven spiders, 80 HP, wind-up web projectile, swept player contact, 2sec snare, 10sec floor webs/48% movement. SpectralWormV33b: 44 isolated frames (16 head/12 trunk/16 tail), transparent SpectralWorm shader animated dark ink, ten segments x200=2000 HP, normal Hurt cap20, PrismWeapon.ApplyingDamage bypasses. Cave.Targets snapshots targets before damage to tolerate splitting during enumeration. Destroyed segment is removed; surviving runs become independent chains with newly assigned mouth/tail, unchanged remaining HP. Head contact20/body10 every .65sec; paths stay in navigable cells; contact cannot pass through walls and also hurts the allied mage. Body width2.5 / spacing1.65. Tail orientation accounts for opposite taper.

Clay appended enum35, StoragePot36 (icons expanded37; all old ids preserved). ShipHold14 clay. Exact7-cell U recipe, main/table allowed, pot inventory2x2/world1x1, internal25 slots/no crafting; placement passes the same contents reference. Buildings reject placement outside cave floor.

DragonRitual.Schedule checks outdoors (coast/field only, rejects drawn roofs/entrances); 7 meat drops golden-angle jitter, distance .32.. .6 beyond the union of all opaque resting poses (spread wings and moving feet included). Tentacle pulse35 vs9. HunterPresentation uses registered authored frames while walking, 48 overlay phases at35% movement; NightHunter signed distance runs backwards on retreat.

V32 included: Caravan.cs pullers same giant-mob200/5sec/80% defence and scale, driver100HP dismount/flee4.4; coast route ship.bounds.max.y+.70. Shared PyromanticExplosionV32 24 frames/1.45sec, row gutters cropped to avoid neighbor strips; sparks initialized tiny immediately (no first-frame white blocks), BlastRing. Dungeon mage5/120 cone. DragonFireColumnV32 fire-only atlas, pixel dither frame transitions. Softer noisy ScorchedGround and nonoverlapping compact actor labels with numeric HP inside.

Focused checks cover caravan/travel/damage and explosions, actual pot crafting/placement/storage, random entrance/entry/map, all cave wall edges, spider projectile contact/web decay, segment health/cap/split/two-chain pursuit/prism bypass, area collider sanity and all-direction axolotl retreat. New images/paths/exact prompts in ART-V32.md and ART-V33.md. No user game processes were closed.

## V31: перспектива, расчистка и наковальня 1x1

299 модельных проверок и 11 точечных runtime-проверок PASSED. Полную историческую серию не повторяли.

BuildingSystem.Footprint для наковальни теперь Vector2Int.one. Предмет в инвентаре остаётся 2x2, world width .91, collider .80x.64. Центр, ghost, рамка выбора, проверки занятости и Cells следуют 1x1. PoseStructure не вращает SpriteRenderer на90: переключает CraftingTableUprightV31 (width .94) и горизонтальный CraftingTableV28 (width1.92). Колесо V30 и два способа переноса V29 сохранены.

BuildingSystem.Ground хранит clearings по world root, убирает цветы, stems/shadow и регистрирует процедурное удаление. ProceduralMeadow.Load дополнительно пропускает IsCleared. Вытоптанная земля — отдельный Quad с BuildingGround.shader, TerrainTiles, world coordinates и мягкой неровной пиксельной кромкой. Создаётся только на Coast/GoldenField, не пачкает интерьер или карманное измерение. У костра небольшой обугленный центр остаётся после взрыва.

ArtBank загружает AnvilV31, CraftingTableUprightV31, CampfireBaseV31. ArcaneCombat.CreateCampfire использует отдельное основание и вызывает CreateClearing; прежние кадры пламени сохранены и подняты на .22. Новые PNG созданы встроенным imagegen, исходные сохранены; пути и точные промпты в ART-V31.md.

RuntimeChecks.V31 подтверждает выбор двух рисунков, колесо, реальное размещение Anvil1x1, удаление цветов/stems/shadows, четыре ground patch, расположение огня и фактическую GPU-разницу земли. Пользовательская V30 не закрывается; запущена новая отдельная V31. Соседний launcher теперь Windows-V31.

## V30: колесо мыши, компактная наковальня и новая смерть босса

296 модельных проверок и 11 точечных runtime-проверок PASSED. Проверены только изменённые сценарии; полная историческая серия не повторялась.

BuildingSystem.HandleWheel поддерживает Input.mouseScrollDelta и EventType.ScrollWheel. Последний обработанный Time.frameCount предотвращает двойной поворот; RefreshPreview выполняется до обработки колеса, чтобы создание ghost не сбрасывало первую прокрутку. ChoosingPlacement охватывает pending и DraggedItem вне интерфейса. В V29 уже добавлены контрастная сетка всего видимого мира и полупрозрачный ground preview; они сохранены.

AnvilV30.png создан imagegen из AnvilV28.png: только металл, без деревянного основания. PoseStructure задаёт ширину 1.15 вместо 1.72, физический коллайдер .98x.72. Логический footprint 2x2 и инвентарь 2x2 остаются прежними.

RoosterDefeatV30.png: 4x4 кадра, восемь подъёма лап и восемь спокойного цикла. DragonArt.LoadRows изолирует компоненты в mesh, MatchHead сохраняет масштаб по гребню, RegisterBelly регистрирует каждый кадр по центру золотого оперения, DefeatRestHeight ставит спрайт на землю. PoseDeath до удара об землю показывает прежний переворот, после — новый текстурный труп с grounded wings, detailed tongue и поднятием лап. Старый LineRenderer языка удалён. Равномерный rest отсчёт сохраняет loot7, one shake, десять секунд и затем dissolve1.5s.

Полные пути, финальные промпты и режим инструмента записаны в ART-V30.md. Старые спрайты и версии не удалены. Пользовательские процессы не закрываются; лаунчер соседнего outputs ведёт в Windows-V30.

## V29: актуальная сетка размещения

294 модельные проверки и13 точечных runtime-проверок PASSED. Полную серию926 проверок не повторяли. Сохраняется весь функционалV28.

Последний запрос: сетку установки стола/костра/наковальни должно быть видно при выборе места. В момент начала работы пользователь запускалстаруюWindows-V26\AshenCoast.exe; не закрывать этот процесс. Актуальный соседнийlauncher теперь ведёт вWindows-V29. Объяснение дано пользователю.

BuildingSystem.PreviewItem выбираетpending илиInventoryUI.DraggedItem. ChoosingPlacement показывает сетку также для предмета, взятогоЛКМ, когда указатель выходит за инвентарь. Чистыйпревью не меняетpending/source: иначе два обработчикаЛКМ могли бы расходовать предмет дважды. RefreshPreview лениво создаётghost, сохраняет ориентацию; скрывает его над инвентарём. PlaceCarried атомарно проверяет и ставит предмет изисходнойсетки; InventoryUI.DropAtPointer больше не запускаетновыйpending,сбрасывающийповорот. DrawDrag внепанели длястроений заменён наземнымпревью. ПКМ-режимустановки сохранён.

СеткаOnGUI depth-5 нижеинтерфейса-10: тёмнаяподложка3px/альфа.66 и светлаяосновнаялиния1px/альфа.66. ВесьвидимыйViewport размечен. Клеткистроения имеюттёмнуюрамку4px и зелёную/красную2px. Новые проверки берут каждый изтрёхпредметов реальным событиемЛКМ, подтверждаютвыходназемлю, разницуотрисовкиghost наGPU,поворотпереносимогостола иустановку ссохранениемориентации. ЕстьотдельнаяпроверкаПКМ иотмены.

## V27: текущие изменения и проверка

Unity-сборка успешна;285 модельных проверок и24 точечных runtime-проверок новых механик прошли. Полная прежняя серия926 проверок была выполнена наV26; наV27 она повторно не запускалась по просьбе пользователя не затягивать проверками.

Подтверждённые ответы пользователя: Q ускоряет только смену дня и ночи, повторная Q или движение прекращает; приHP<20 медитация запрещена и прерывается. Размытие заметнее накраях, герой/GUI чёткие. Музыка боя заменяет музыку локации. Последняя жалоба: пламя отстаёт от летящего босса. Исправлено движущейся привязкой источника и конца струи к текущему RoosterDragon вплоть до контакта с землёй.

DragonRitual.Tick делит шаги на1/60s, совместно обновляя босса и FallingDragonFire. Частицы доimpact.source.Mouth и.source.FlightImpact актуализируются каждый шаг; послеimpact наземное пламя остаётся там, где упало. FlightImpact=Mouth+aim*(Altitude+3.6+|velocity|*.18)-up*Altitude, полёт.16s, эмиссия.025s. BurnCone и радиусный урон удалены из реальной атаки. DragonFireMote выжигает по текущему видимому следу; HurtByFireContact проверяет SpriteContact торса/ног и временной интервал. Отдельное пространство земли больше не наносит урон.

Meditation — отдельное состояние с Blend/DaySpeed/DayOffset. seconds продолжает идти обычно; DaySeconds=seconds+DayOffset используется только для света/смены дня/ночи и ночных существ. Ни Time.timeScale, ни dt боя не ускоряются. Посадка использует настоящий геройский торс и кодовый пиксельный рисунок сложенных ног вWorldSprite._SeatedLegs; геометрия героя не сдвигает Rigidbody. PixelAtmosphere/Atmosphere.shader — размытие с защитой героя, интерфейс рисуется поверх отдельно, местами марево жара в бою. Soundscape переключает DragonViolinV27.wav: оригинальные51.2s/150BPM/стерео, составлено work/generate_audio_v27.py.

Всегда сохранять активные процессы игрока: не закрывать принудительно. Пакет обновляет свободныеWindows и соседнююWindows-V27; launchers указывают актуальную сборку. СтарыеV24–V26 не удаляются. Пользователь просит обновить и открыть без длительных проверок. Открывать готовую игру обычным видимым окном; фоновые помощники толькоHidden. У игровых состояний нет сохранений.

## V28: текущая версия

Сборка успешна:294 модельных проверок и33 точечных runtime-проверок PASSED. Пользователь просит ограничиваться необходимыми проверками и обновлять/открывать игру. Старую полную серию926 проверок повторно не запускали. ПрежниеV26/V27 разделы ниже — исторические; новое поведение имеет приоритет.

Подтверждён ПКМ для экипировки и ЛКМ по слоту для взятого предмета. Новый EquipOrDrop возвращает прежнюю вещь в основной инвентарь, при нехватке места передаёт её в ScatterLoot; на землю она вылетает с анимацией. ЛКМ по сетке остаётся переносом. GroundLoot также экипирует подходящие предметы приПКМ. Крафт:2 дерева→костёр,3→стол3×3. IronIngot2×1,5 штук в трюме. Точная схема наковальни из фото: позиции левых клеток слитков(0,0),(2,0),(1,1),(0,2),(2,2), область4×3. Допускается только в инвентаре сcraftLimit4; неправильная схема не расходует материалы. Стол имеетcraftLimit4, обычные сетки3, наковальняcraftingEnabled=false.

BuildingSystem — общая установка всех структур, полная видимая сетка, прозрачныйghost, точное целочисленное основание, коллизии. Костёр1×1; столвертикальный1×2/горизонтальный2×1 (колесо); наковальня2×2. CraftingStation хранит собственную GridInventory5×5; InventoryUI.Stations выводит её слева от основного инвентаря, анвил имеет термометр и нагрев от соседних костров. ArcaneCombat сохраняет старые публичные методы установки как обёртки для совместимости.

DragonArt.corpse использует исходный RoosterWalkV24c с центральным pivot и изолирующеймаской. RoosterDragon.PoseDeath выполняет зарегистрированный переворот/падение/отскок/язык. TickDeath приimpact вызываетShakeCamera ровно раз иScatterLoot7×RawMeat2×1. Десять секунд после падения труп непрозрачный; затемWorldSprite._CorpseDissolve иDragonDust улетучивают его за1.5s. Finished только послеfallDuration+11.5. Важно: UnityMathf.SmoothStep принимаетfrom/to/t, поэтому альфу считать черезInverseLerp для порогов. BurnFlowers срабатывает отреальновыжженной клетки; picked=true/RecordPickup предотвращают восстановление после выгрузкичанка.

Новое пользовательское решение: медитация ускоряет всё. Meditation.Tick вызывается сunscaledDeltaTime и постепенно устанавливаетTime.timeScale от1 до5 за4реальныхсекунды. DayOffset=0, отдельное ускорение не добавляется. Stop/HP<20 возвращаетTime.timeScale1;пауза0. Медитационная силаразмытия.16, марево.15; герой/GUI защищены. Soundscape используетDragonViolinV28.wav, выразительная синтезированнаяскрипичнаякомпозиция51.2s/150BPM/стерео;темп аудио не ускоряется. Полная документациягенерации вART-V28.md, исходникиPNG неизменены.

СвежиеNativeWindows/соседниеWindows иWindows-V28 копируются без закрытия пользовательских процессов; активные папки пропускаются в пользуV28. В обоихoutputs естьЗапуститьигру.cmd. Никогда не завершать игру пользователя: состояния не сохраняются.

## Где лежит проект

- Рабочая папка: `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3`.
- Настоящий Unity-проект: `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\AshenCoast`.
- Актуальная игра V33: `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\Windows\AshenCoast.exe`. Вся соседняя папка `AshenCoast_Data` необходима для запуска.
- Unity Editor: `C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe`.
- Архив исходников: `outputs\AshenCoast-Unity-Project.zip`; архив игры: `outputs\AshenCoast-Windows.zip`.
- Подробности требований и изменений: `outputs\ПРОЧТИ-МЕНЯ.md`; точные запросы и выбранные растровые исходники: `outputs\ART-PROMPTS.md`.
- Промежуточная работа находится в `work`. Сборка V23 находится в `C:/Users/tigra/Documents/Codex/2026-10-11/unity-c-users-tigra-documents-codex/work/Windows-V18` и скопирована с проверкой SHA256 в `C:/Users/tigra/Documents/Codex/2026-10-11/unity-c-users-tigra-documents-codex/outputs/Windows`. Прежняя папка Windows в старом чате тоже обновлена до V23. Редактировать нужно тот же Unity-проект выше.

## Что это за игра

Нативная Windows-игра на Unity с детальной пиксельной графикой и видом 2,5D, по настроению близкая к Stoneshard. Пользователь просит взрослые пропорции персонажей, узкие естественные ноги, отчётливое переступание и качественные анимации. В игровом интерфейсе нужны только необходимые подписи. Общение — на русском.

Реализованы пляж с большим кораблём-хранилищем, поле собираемых цветов, анимированный прибой, стартовые туман и гроза, цикл дня и ночи по 180 секунд, ночные светлячки; карманное измерение с космосом, галактикой, планетой, руинами и анимированной дверью возврата; поездка на деревянной повозке с двумя мокрицами в золотое поле с тополями и таверной. В таверне есть отдельный склад, торговец, пьяница, спящий посетитель, маг, беседующие посетители, девушка за работой, камин, ковры и столы. Союзного мага можно нанять.

Инвентарь игрока 10 столбцов × 5 строк. Предметы занимают разные площади, обычные предметы не складываются. Монеты — исключение, складываются по 10; стартовый запас 10 монет. Восемь слотов снаряжения, включая обувь. Инвентарь открывается E справа; мешок 5×5 и трюм открываются отдельно правой кнопкой и появляются левее. ЛКМ поднимает/перемещает предмет, возможен обмен с занятым местом; ПКМ ест еду, открывает книгу либо экипирует предмет в пустой совместимый слот. Есть выбрасывание предметов и установка костра по клеткам при сохранении возможности ходить.

Здоровье, иммунитет и мана имеют максимум 100. Мана начинает с 0 и сама не восстанавливается; мозговик даёт 100 маны и не занимает ячейку насыщенности. Пять ячеек уникальной еды дают бонусы. Есть крафт верёвки из двух цветов и костра из двух древесин, через выделение ресурсов ПКМ в инвентаре. У снаряжения прочность, у ламп также топливо. У книг прочности нет. В открытом виде пространственный фолиант и книга щупалец занимают 3 столбца × 2 строки. Огненная книга сохраняет площади 1×2 закрытая / 2×2 открытая; увеличивался только её рисунок.

Есть меч, кинжал, коса, копьё, йо-йо, сфера и лампа; тренировочный манекен, мокрицы и лягушка-маг. Урон ближнего боя привязан к контакту текстуры оружия с целью. Мокрица сворачивается на 5 секунд, в клубке поглощает 80% урона и при смерти оставляет мясо. Сфера рисует светящийся след с частицами даже при движении. После перестановки символов пятиконечная звезда вызывает карманное измерение, исходный пересекающийся знак — щупальца. Для заклинаний нужны соответствующие открытые книги. Карманное измерение стоит 20 маны, щупальца 30, взрыв костра 10; лампы расходуют ману и топливо, взрыв союзного мага — 40 его маны.

## Новое в V26: небо, обширное выжигание, лучи и звуки

Финальная V26:285 модельных и926 runtime-проверок PASSED (910 прежних +16 новых), итоговый осмотр Verification/visual-review-v26.md.

Последний запрос повторён пользователем и полностью принят: небесный столб, более массовое выжигание, опережающий огонь летящего босса, подробный сфокусированный луч, свет вокруг каждого луча, начало из кончика, лазерный звук и звук попадания, самостоятельные кристальные частицы черчения. Пользователь также попросил запустить и уточнил путь; была запущена V25. Его замечание о размере объясняется старой Windows-сборкой из соседнего чата: DLL там была от2:00, в Windows-V25 от2:22. Снижение спрайта на30% V25 подтверждено и сохраняется; повторно его не уменьшаем.

DragonRitual.PoseColumn поддерживает высоту>=14 и на4 единицы выше верхнего края камеры. HearthFire имеет отдельную ветку _SkyColumn: исходные24 подробные рисунка используют две перекрывающиеся смещённые полосы, чтобы убрать стыки повторения; движение текстуры и световых прожилок направлено вниз. Прежний камин с _SkyColumn0 сохранён. При призыве BurnArea radius4.3; BurnCone reach13, ширина.8+.4d; при воздушном попадании radius2.2. До768 клеток с кешем Dictionary world/cell, жизнь10s, плавное затухание и исключение воды/внешнего края карманной комнаты. Пятна1.9×1.6 перекрываются в непрерывную землю.

FlightVelocity вычисляется по движению в каждом малом шаге. Место удара=Position+FlightVelocity*.22+aim*3.2; полёт огня.22s, эмиссия каждые.045s. Движение линейное, исходник вращается по направлению струи; приземлившиеся языки живут ещё.80s. Mouth обновляется через текущую фазу Pose до каждой эмиссии, поэтому огонь следует за головой.

PrismWeapon.Origin теперь настоящий bounds.max.y без множителя.75. Лучи имеют4 вершины; нормированная кешированная widthCurve даёт старт.008 и набор ширины в горле.18 для тонких /.68 для общего. widthMultiplier держит прежний полный размер. Physics/Damage используют конечную вершину3. Детали PrismBeam — пять изменяющихся нитей, световые пакеты, блёстки, белая сердцевина, цветная оболочка; длина материала соответствует физической длине до препятствия.

PublishLight передаёт до7 точных отрезков в _PrismSegments/_PrismBeamStyle. CoastLighting вычисляет минимальное расстояние до каждого отрезка и непрерывное голубоватое освещение (радиус1.15 для тонких и2.6 для общего), использует максимум при перекрытиях для сохранения деталей пола. Обычные32 лампы/светлячки сохранены отдельно. Глобальные данные очищаются при затухании, смерти компонента и новой игре.

PrismAudio создаёт стереофонический44.1kHz двухсекундный бесшовный лазерный синтез и короткий энергетический удар. Source подчиняется AudioListener/F10, меняет pitch/volume по Charge и плавно отключается. ApplyingDamage ограничен try/finally вокруг контакта; CombatFeedback выбирает новый Impact только для урона призмы, остальные эффекты сохранены. SpatialMagic.SilverInk имеет самостоятельные ромбические гранёные Crystal, вращение, орбитальный дрейф, изменённые частицы на линии и отблески; у сферы прежние квадратные мотыльки.

PrismSkyChecks проверяет GPU-свет у всех6 лучей, геометрию кончика, живые нити, настоящий звук попадания, стереоволну, отключение света/звука, широкий след и эффект черчения. Расширены прежние проверки неба, масштабного выжигания и упреждения. Сборка work/Windows-V26, финальный лог work/unity-build-v26c.log; новый полный тест work/Verification-V26. package_v26.py сверяет свежесть и SHA256, сохраняет работающие игры, создаёт стабильный Запустить игру.cmd. В соседнем чате он указывает на Windows-V26, здесь — на свежую Windows. Резервная копия источников перед работой work/v25-before-sky-fire.zip. Новых растровых ресурсов не создавали: использованы прежние подробные текстуры с новыми материалами.

## Новое в V25: уменьшенная призма и хаотичное сведение

Финальная V25:285 модельных и910 runtime-проверок PASSED (895 прежних +15 новых). Осмотр: Verification/visual-review-v25.md.

Последний запрос: уменьшить спрайт примерно на30%, направлять предмет вдоль выстрела, оставлять старую стойку без стрельбы и сделать сведение лучей менее равномерным. Нового растрового рисунка нет: подробная призма и её опаловые переливы V24 сохранены.

ArtBank.PrismVisualScale=.70; единый масштаб применяется в DrawIcon (все места интерфейса), Combat.SetupWeapon (ширина.322 вместо.46) и CreateLoot (выпавший предмет). Footprint остаётся2×2. Combat.PoseHeldPrism подготавливает спрайт и его позу; вершина+Y смотрит вдоль PrismWeapon.Angle, без выстрела rotation0. PrismWeapon имеет DefaultExecutionOrder1100. Pose синхронизирует предмет перед вычислением Origin; LateUpdate повторяет позу после финальной gait-позы героя. Лучи и предмет используют одну конечную точку руки. Остаточное затухание лучей хранит lastBeamOrigin, а отсутствие эффекта не делает ненужные физические трассировки.

RayOffset вычисляет разный профиль сведения для каждого из6 лучей, плавную многосоставную угловую волну с возвратными движениями и затуханием к1.8s. Начальный общий веер60°, финальное смещение0; серия залпов меняет focusSeed без изменения глобального Random. RayFlowSpeed даёт каждому лучу независимую переменную скорость, flowPhase интегрируется по малым шагам и передаётся отдельным MaterialPropertyBlock в PrismBeam. Общий луч после фокусировки сохраняет прежнюю устойчивую анимацию и его собственная линия теперь используется для проверки попадания.

PrismFocusChecks добавлен в PrismDragonChecks: настоящий размер в мире,8 направлений с проверкой оси и вершины, совпадение лучей с рукой при движении, возврат стойки, неодинаковые скорости/траектории и итоговое сведение. Снимки v25-prism-aim-0..7.png и v25-focus-phase-00..05.png. Сборка work/Windows-V25, проверки work/Verification-V25, лог work/unity-build-v25c.log. package_v25.py копирует проверенные файлы с SHA256. Запущенная игра в прежней папке соседнего чата сохраняется; обновление размещается там в Windows-V25. Основная папка Windows этого чата обновлена. Резервная копия пяти исходников перед изменениями — work/v24-before-prism-motion.zip.

## Новое в V24: призма, призыв и огненный босс

Финальный полный прогон: 285 модельных и 895 runtime-проверок (848 прежних +47 новых), все PASSED. Логи work/unity-build-v24g.log и work/player-test-v24.log; осмотр Verification/visual-review-v24.md.

Последний запрос текущего чата выполнен поверх V23: добавлена Последняя призма по обновлённому референсу и переливам слитка; сильно расширен столб призыва; создан враждебный петух-дракон. V23, прежняя земля, карта, форт, аксолотль, интерфейс и T/U сохранены.

PrismWeapon: ItemKind.LastPrism добавлен в конец перечисления (индекс30, старые сохранены), 2×2, стартовый инвентарь. LMB: шесть лучей60°, ChargeSeconds1.8, ThinDrain6.5/s, FusedDrain28/s, FullDps420, Range>=2 диагоналей камеры. SmoothDampAngle .22s /230°s, пиксельные потоки в PrismBeam. Physics преграды остаются, коллайдеры персонажей и манекена пропускаются. Реальные попадания идут по непрозрачным пикселям спрайтов. Combat держит и поворачивает призму; новое свойство _Prism отдельно от _Orb.

SpatialMagic.HasOrb допускает исправную сферу или призму; SilverInk фиксируется в начале рисования. InkColor окрашивает ленту и частицы серебром. Призма распознаёт только звезду пространственного фолианта; прежний символ щупалец и открытая книга без сферы ничего не вызывают. Мана20 и возвратный портал сохранены. PrismSurface.cginc используется и WorldSprite, и PrismIcon; RenderTexture64×64 Point. LastPrismV24c — выбранное финальное изображение.

DragonRitual.Schedule вызывается настоящим TryExplodeFire. Через2 секунды колонна4.7×7.4 с материалом HearthFire и24 рисунками; ещё через3 секунды Spawn1500HP. Summons считаются даже при смене локации, а существа двигаются только в активном мире. FlightInterval20, FlightDuration6; огонь13 единиц, полёт с нисходящими струями, пек32 и лапа44 в фазе контакта. Mouth определяется по жёлтому клюву; полётные кадры привязаны к профилю по этому якорю. ScorchedGround — кодовый пиксельный уголь/зола с круглым неровным краем, только на суше, жизнь10s. Белый спрайт использует весь размер встроенной whiteTexture для UV0..1.

DragonArt читает RoosterWalkV24c и RoosterAttacksV24c (4×6), RoosterFlightV24c (4×4), DragonFireColumnV24b (6×4). Связные силуэты распределяются по четырём главным телам в ряду; полные хвосты/крылья сохраняются через Sprite.OverrideGeometry и RegisterHitMask. PPU фиксирован для группы, масштабы атак/полёта сверены по гребню; опорные точки не привязаны к поднятой лапе. В редакторских model checks OverrideGeometry пропускается (Unity запрещает вне playmode), в native применяется.

Добавлены PrismDragonChecks: настоящая сборка, цветовой материал на GPU, серебряные рисунки, урон/мана/задержка, фазы призыва, атаки, повторные взлёты, выжженная земля и смерть. В полном тесте идут после DungeonFeedbackChecks. Быстрая самостоятельная проверка доступна флагом -v24-checks-only вместе с -coast-test; фотографии v24-*.png. Основная свежая сборка work/Windows-V24, полный тест work/Verification-V24. package_v24.py проверяет свежесть отчёта, копирует native в Windows двух чатов с SHA256, обновляет оба архива и документацию. Резервная копия исходников до этого изменения — work/v23-before-prism-dragon.zip. Референсы сохранены в References-V24, полные запросы — ART-V24.md.

## Новое в V23: форт, попадания и анимация охотника

Последний запрос выполнен: исправления трёх снимков, проверка хитбоксов данжа, единый стиль разбойников, вспышки HP и звуки попаданий, заметный спуск и общая подсветка, больше кадров аксолотля. Подробности и фактические проверки — Verification/visual-review-v23.md, runtime-verification.txt. 277 модельных и 848 runtime-проверок прошли.

FortWorld.Stone использует ArtBank.AddStoneFootprints(sr,.55), регистрирует основания в stoneColliders и HashSet solids. Проходы оставлены открытыми. Движение врагов использует невыделяющий CircleCast со 128 результатами; ближние атаки, стрелы и магия учитывают камень. Северный предел тела16.3. Внешний спуск FortDescentV23 и тёмный силуэт за аркой; клик по всему фасаду теперь не считается входом. Старые стрелы очищаются при Enter/Leave. Тесты реально двигают Rigidbody2D, в том числе T×5.

CombatFeedback хранит время попадания по SpriteRenderer, Flash(.48s), Recent(.6s), шесть аудиоголосов, отдельные PlayerImpact/MobImpact/ShellImpact. Все семь боевых типов используют ShowDamage с renderer. InventoryUI показывает вспышку в HP и короткую смертельную шкалу. InteractionFeedback обрабатывает _Hover/_InteractPulse, выбор объекта и импульс успешного действия, включая прибытие через портал.

ThreatArt.LoadBandits читает BanditActionsV23.png с шестью измеренными рядами [0,194,369,553,719,876,1024], выделяет восемь главных связанных силуэтов и прилегающие части. Sprite.OverrideGeometry сохраняет полное оружие и ноги без соседних кадров. ArtBank.RegisterHitMask/Opaque используют тот же силуэт для попаданий. PPU фиксирован для роли по максимальной высоте ходьбы; атаки не меняют масштаб. Промпты и выбранные изображения — ART-V23.md. Квадратная проба imagegen не использована.

NightHunter.MovementFrames=48, MotionFrame зависит от пройденного расстояния. GaitMoving использует базовый рисунок0 и четыре независимых движения LegMotion/LegWeights. Лицо в каждом из трёх ракурсов остаётся неподвижным. Старые 8 рисунков доступны для ручных поз и старых тестов. HunterPresentation исправляет _TailPivot/_PawPivot (раньше ошибочно _hunter.*), передаёт четыре движения лап через UV1 и IDs параметров. Axolotl.shader target3.0. Требования текста/лампы/ночи/размера сохранены. Все 48 фаз проверены настоящей отрисовкой; код не утверждает, что это 48 новых нарисованных изображений.

Земля и трава V20 сохранены: terrain-restoration-v23.json, 15 проверок. Unity объекты свойств CoastGame/JourneyWorld теперь создаются в Awake/Initialize, поэтому ошибки сериализации исчезли. Сборка использует LZ4HC без изменения текстур. Двадцать устаревших листов из Resources перемещены вместе с .meta в Assets/SourceArtArchive; PNG/метаданные проверены SHA256, ничего не удалено. Это уменьшает runtime-ресурсы, исходные картинки входят в архив проекта. Старые каталоги work/Windows-V17 и проверки прежних выпусков сохранены.

V23: `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows\AshenCoast.exe`. Обновлённые native-папки: C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows, C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\Windows. Оба архива обновлены, файлы в папках проверены SHA256. Если папка Windows была занята пользовательским процессом, отдельная Windows-V23 становится текущей; процесс не закрывается. Неиспользуемые отдельные native-ресурсы прежнего выпуска сохранены; текущая игра загружает полный контейнер data.unity3d. ZIP включает только файлы проверенной V23. Политика выполнения скриптов Windows не менялась.

Промежуточные каталоги work/Windows-V18 и work/Verification-V20 теперь содержат V23. Логи work/unity-build-v23-final.log и work/player-test-v23.log. Скрипты run_v23_checks.py, package_v23_release.py; RuntimeChecks.Dungeon.cs добавляет проверки. Резервная копия до изменений work/v22-before-dungeon-feedback.zip. patch_v23.py, fort_ui_v23.py, hunter_v23.py, checks_v23.py, repair_v23.py, finish_v23.py, register_bandits_v23.py и silhouette_checks_v23.py — одноразовые патчи, повторно не запускать.

## Новое в V22: прежняя земля и исправления снимков

Последний запрос: исправить проблемы на трёх снимках и в точности вернуть прежнюю землю/траву на всех локациях. Terrain и JourneyFloor восстановлены байт в байт из work/v20-before-polish.zip. Другие шейдеры пола, освещения и атмосферы совпадают с этой копией. WorldSprite вернулся к исходной насыщенности; ArtBank.PixelStyle — к дробному шагу 40 PPU; PixelAtmosphere — к половине размеров окна и Point-фильтрации. Растровые текстуры не менялись. Таким образом, изменения земли и травы V21 отменены. Пропорции и анимации аксолотля, интерфейс и структурные исправления V21 сохранены.

Чёрный провал в форте возникал из-за конечного пола и неограниченного следования камеры. FortWorld.FloorBounds теперь (-30,-22)..(30,26); каменная подложка расширена за стены. Walkable bounds и стены прежние. CoastCamera ограничивает целевую и сглаженную позиции по видимой части сцены; инвентарь не прячет героя у восточной стены. PixelAtmosphere и PixelPresenter обновляют полный viewport при изменении окна и полного экрана. InventoryUI размещает подписи существ целиком внутри видимой сцены и не рисует их поверх шкал героя, ячеек насыщенности или панели союзного мага.

Светлые квадраты на фото — монеты после смерти разбойников. В мире они теперь .26 единицы вместо .62, с круглой маской _GroundCoin, перспективным сжатием и небольшой тенью. Маска действует только на выпавшие монеты. Инвентарная иконка, гравировка, стоимость, число и подбор прежние.

Успешная сборка: 277 модельных и 731 runtime-проверок. FortPhotoChecks проверяет реальные четыре угла форта в пяти режимах экрана, отсутствие чёрных полос, видимость героя с инвентарём, фактические границы HUD, рисунок и подбор монеты. Просмотрены v22-*.png. Исходники/текстуры проверены отдельно: Verification/terrain-restoration-v22.json; визуальный отчёт: visual-review-v22.md. Свежие измерения трёх сценариев — performance-v22.md/csv.

Актуальная игра V22 находится в `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows\AshenCoast.exe`. Прежняя папка `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\Windows` также обновлена до V22. Оба архива обновлены. Все native-файлы в обеих папках проверены SHA256; пользовательские процессы не закрывались. Исторические work/Windows-V18 и work/Verification-V20 переиспользованы; сейчас в них V22. Логи: work/unity-build-v22-final.log и work/player-test-v22.log. Скрипты: work/run_v22_checks.py и work/package_v22_release.py. Резервная копия до изменений — work/v21-before-fort-fixes.zip. fix_v22.py и checks_v22.py — одноразовые патчи, не запускать повторно.

## Новое в V21: графика, опора, интерфейс и структура кода

Выполнен запрос исправить все замечания визуального и технического обзора. WorldArtStyle задаёт 32 пикселя/единицу. PixelAtmosphere создаёт RenderTexture по мировому масштабу камеры, а не половине окна; материалы спрайтов и аксолотля используют общий шаг выборки. Terrain и JourneyFloor сглаживают мелкий контраст несколькими выборками; трава имеет более сдержанную палитру. WorldSprite и Axolotl слегка уменьшают насыщенность, сохраняя рисунки.

ThreatArt.RegisterHunterFeet регистрирует кадры аксолотля по нижним непрозрачным когтям, использует постоянный PPU для всех фаз одного вида. HunterPresentation.Pose применяет равномерный масштаб, профиль длиной ProfileLength=8.53; отдельное растяжение высоты удалено. Боковой ракурс ниже фронтального из-за естественной позы. Добавлены четыре контактные тени и общая тень под корпусом. Сохранились габариты профиля, фазы бега, неподвижная морда, жабры, хвост, одна скребущая лапа, реплика 5/4/15 секунд и очень тёмный фиолетовый без белого. Axolotl.shader даёт слабый холодный свет по двум направлениям края; ночь GoldenField не осветлялась глобально.

InventoryUI: общий HUD, небольшие гравированные знаки, акцент здоровья, крупные числовые значения, компактная насыщенность, согласованные рамки. World labels крупнее, с тёмной подложкой и тонкой полосой HP. Кешируются GUIStyle, строки числовых значений и размеры подсказки; HUD и мировые подписи рисуются только на Repaint. Координаты взаимодействий инвентаря сохранены.

CoastEnvironment отвечает за свет, погоду и береговой звук; CoastCamera владеет состоянием следования камеры; CoastGame.World.cs содержит сборку мира. HunterPresentation владеет сетками и контактом, HunterWhisperView — текстом/частицами и кешем букв. NightHunter сохраняет AI, таймеры, здоровье и исключение света. Проверка стен использует ограниченный буфер CircleCast вместо CircleCastAll/Any; зоны света переиспользуются. ProceduralMeadow.Refresh пересчитывает план только при смене участка или размера обзора; загрузка ещё идёт постепенно. FlowerAt теперь без LINQ-сортировки, HighlightAt переиспользует блок материала. Освобождаются материалы, текстуры и звук новых систем.

Успешная сборка: 277 модельных и 648 runtime-проверок. Просмотрены реальные v21-*.png, актуальные v20-paw-* снимки анимационных тестов. PerformanceSurvey снимает CPU/кадры, число draw calls и память для трёх сценариев по 180 кадров. Неподдерживаемые счётчики выделений обозначены NaN; наличие счётчика проверяется реальным выделением 64 КиБ. Короткое измерение не заменяет длительную проверку на других компьютерах. Отчёты: Verification/visual-review-v21.md и performance-v21.md/csv.

Настоящий проект: `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\AshenCoast`. Текущая игра: `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows\AshenCoast.exe`. Архивы обновлены до V21. Исторические имена промежуточных папок work/Windows-V18 и work/Verification-V20 сохранены для экономии места. Логи: work/unity-build-v21-final.log, work/player-test-v21.log. Упаковка: work/package_v21_release.py; проверки: work/run_v21_checks.py. До изменений сохранён work/v20-before-polish.zip. revise_v21.py, architecture_v21.py, interface_v21.py, verification_v21.py и allocations_v21.py — одноразовые патчи, не запускать повторно. Пользовательские процессы не закрывались.

## Новое в V20: хвост, одна скребущая лапа, задержка 5 секунд

NightHunter.cs / Axolotl.shader: цветовые каналы сетки теперь содержат вес жабр, сторону фазы жабр, вес хвоста и вес одной передней лапы. TailWeight и PawWeight зарегистрированы отдельно для трёх видов. Хвост вращается с нарастающим к концу весом вокруг TailPivot, остальные области остаются жёсткими. _TailTime отделён от _GillTime для независимой проверки. Сетка отражается целиком, как в V19. Голова не получает вес хвоста/лапы.

IdleSeconds накапливается при нулевом перемещении хищника и сбрасывается при движении, Warp, рассвете и уходе из области. StandDelay=5, ScratchCycle=3.8. PawMotion даёт мягкие ключи подъёма, выноса, постановки и скребка одной передней лапы: два движения по .75 секунды, затем 2.3 секунды паузы. Поворот и перенос применяются только через вес PawWeight. TickScrapes использует ограниченный пул из 32 частиц земли и четырёх бороздок; они появляются в фазе контакта с землёй и затухают. ScrapeCount считает реальные скребки. Частицы находятся в отдельном корне поля, их координата вычисляется по текущей деформации лапы, учитывая отражение. Материал/текстуры освобождаются при уничтожении.

Начальная задержка реплики изменена с 7 на 5 секунд ожидания с лампой. Длительность 4 секунды и интервал повторения 15 секунд сохранены. WhisperInk использует только очень тёмный фиолетовый: InkDark (.16,.035,.25), InkLight (.25,.065,.37). Убраны белые/светлые оттенки из букв и частиц; контур, ореол и украшения также тёмно-фиолетовые.

HunterIdleChecks проверяет фактическое изменение хвоста при замороженных жабрах, изменение выбранной лапы и неподвижность второй лапы/морды в трёх видах, отсутствие скребков до пяти секунд, первую реплику на пятой секунде, бороздки, сброс при ходьбе и рассвете, тёмную палитру. Проверка ускорения T теперь пропускает кадр после смены тестового ввода, чтобы Update успел передать направление FixedUpdate. Это устраняет зависимость теста от нескольких физических шагов в одном кадре при загрузке большого поля; сама скорость не менялась.

Сборка успешна: 277 модельных и 618 runtime-проверок. Просмотрены снимки v20-tail-*, v20-paw-*, v20-dark-violet-whisper.png. v20-scrape-preview-0/1/2.gif собраны из фактических 32 кадров рендерера; повышенный свет используется только для осмотра движения, не в обычной игре. Отчёт — Verification/visual-review-v20.md.

Исходный Unity-проект остаётся `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\AshenCoast`. Актуальная игра — `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows\AshenCoast.exe`. Оба архива обновлены до V20. Папка work/Windows-V18 снова использована для новой версии; её имя историческое. Логи: work/unity-build-v20-final.log и work/player-test-v20.log. Проверка: work/run_v20_checks.py; упаковка: work/package_v20_release.py. До правок сохранён work/v19-before-idle-tail.zip. patch_v20.py и checks_v20.py — однократные патчи, не запускать повторно.

Все изменения текущего запроса выполнены; пользовательские процессы не закрывались.

## Новое в V19: кириллица, неподвижная морда, жабры и реплика

Пользователь указал на испорченный экран смерти и желеобразную деформацию головы. В InventoryUI.cs две строки были буквально записаны с mojibake; восстановлены «Вы погибли» и «Начать заново», выделены DeathTitle/RestartLabel. Новые и изменённые C# файлы UTF-8 с BOM. Аудит строк остальных C# файлов не нашёл других восстанавливаемых ошибок этого вида.

NightHunter.Pose использует сетку 64×80 с цветовым весом только внешних жабр из GillWeight(view,uv). Axolotl.shader больше не использует широкую синусоидальную маску по всей голове: деформация умножается на зарегистрированный вес. Лицевая область остаётся неподвижной; внешние жабры покачиваются даже в стойке. Изображения и фазы бега V18 сохранены, нового растрового рисунка нет.

HeadTop вычисляется по положению головы для каждого вида и отражения, а не по верхнему краю полного полотна с хвостом. QuoteAnchor следует за HeadTop+.48. QuoteDuration=4, QuoteInterval=15. Первая реплика после семи секунд стояния с целой заправленной лампой; далее повтор при сохранении этих условий через каждые 15 секунд от старта предыдущей. Speak увеличивает QuoteCount и задаёт кулдаун. Шрифт Gabriola/Georgia 42 italic, контур, фиолетовый ореол, бело-фиолетовое мерцание букв, непрерывно исходящие искры с дрейфом и затуханием. Последняя половина секунды — исчезновение; общий показ ровно четыре секунды. Полосы союзника скрываются по QuoteVisible.

CoastGame.ToggleFastMovement / KeyCode.T: переключатель скорости ×5, повторное нажатие возвращает обычную. MovementSpeed используется фактическим FixedUpdate, включая нормированный диагональный ввод; скорость боя также умножается на пять. StartNight / KeyCode.U переводит seconds на фазу 210 текущего либо следующего цикла без возврата времени назад, сразу обновляет освещение и хищника. В паузе, смерти и поездке быстрые клавиши не действуют.

Сборка успешна: 277 модельных и 598 runtime-проверок. HunterRevisionChecks проверяет кириллицу скомпилированных строк, физическую скорость в восьми направлениях и отключение ускорения, немедленную ночь, длительность и повтор реплики, фактические пиксели неподвижной морды и движущихся жабр в трёх видах. Просмотрены снимки v19-*.png и visual-review-v19.md.

Актуальная Windows-игра: `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\outputs\Windows\AshenCoast.exe`. В процессе работы пользователь запускал игру из папок обоих чатов; пользовательские процессы не закрывались. В уже запущенной игре код остаётся старым до перезапуска. Предыдущая папка `C:\Users\tigra\Documents\Codex\2026-10-10\2-5-d-unity-stoneshard-3\outputs\Windows` содержит V18. Оба архива обновлены до V19. Настоящий Unity-проект остаётся прежним. Промежуточная папка `work/Windows-V18` повторно использована для V19 из-за свободного места; её имя не отражает версию содержимого.

Логи: `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\work\unity-build-v19c.log`, `C:\Users\tigra\Documents\Codex\2026-10-11\unity-c-users-tigra-documents-codex\work\player-test-v19.log`. Проверка: work/run_v19_checks.py; упаковка: work/package_v19_release.py. Предыдущие исходники до правок сохранены в work/v18-before-hunter-fixes.zip. Не запускать повторно patch_v19.py и checks_v19.py: это однократные патчи. Незавершённых задач этого запроса нет.

## Новое в V18: форма суши, форт, ночной хищник

Выполнен запрос пользователя о неровном мире, случайном разрушенном форте и огромном аксолотле возле таверны. Уточнения пользователя: аксолотль высотой 4–5 героев, длиной около трети ширины обычного экрана; защита — только целая лампа с топливом в руке.

`WorldShape.cs` задаёт берег по сумме угловых волн и зерну; южная опора гарантирует сохранность стартового пляжа, корабля и старого поля. Шейдер Terrain повторяет ту же формулу. Цветы создаются только на суше, основание форта очищено. `FortPosition` детерминирована зерном, расположена минимум в 48 условных единицах от края. Карта показывает силуэт суши, раскрывает окрестности посещений, всегда рисует значок форта (также у края обзора) и имеет кнопку «К форту». Посещение форта — отдельная вкладка.

`FortWorld.cs`: отдельный `WorldArea.Fort`, каменный двор 40×32 с помещениями, колоннами, алтарём, оружейными стойками, сундуками, бочками, спальными местами, клеткой и обломками. 13 живых факелов/жаровен, холодный окружающий свет и синтезированный шум сквозняка. Вход через центральный проход внешнего форта, выход через южные ворота. 16 врагов с 100 HP: десять воинов, пять лучников, один маг. Воин предупреждает о рывке, затем движется со скоростью 12 единиц/с; сквозные проверки движения не дают пройти через каменные стены. Лучник стреляет летящими стрелами с проверкой всей пройденной линии; урон 22. Маг заряжает взрыв 1,05 секунды и создаёт существующий эффект взрыва с враждебной областью радиуса 3 и уроном 55. Мечи, йо-йо, огонь, взрывы и щупальца игрока действуют на новых врагов; союзник также атакует их. Погибшие разбойники оставляют монеты. Шесть маленьких крыс едят и убегают.

`NightHunter.cs`: только ночью и только снаружи таверны в GoldenField. Быстрый бег 11,5 единиц/с, одно касание наносит 1000 урона. Целая лампа с топливом сдерживает его на расстоянии 7,4 единицы даже при нулевой мане. Пустая и сломанная лампа не защищает. Избегает сильного света костров и окна таверны; слабая лампа союзника не заменяет защиту игрока. Есть три вида восьми кадров полного тела, шаг зависит от пройденного расстояния, звуки лап, гибкая сетка жабр и собственный шейдер Axolotl. Сетки кадров кешируются. Высота фронтального вида 7,5, профиль вытянут до высоты 7,5 с длиной полотна 8,53; масштаб в мире фиксирован, при изменении размеров окна не скачет. У хищника 2000 HP без полосы в интерфейсе, на следующую ночь новый спавн. Днём и в помещении его нет.

После семи секунд неподвижного ожидания с лампой на дистанции угрозы звучит текст «это не будет длиться вечно». Текст зафиксирован в мире две секунды, переливается бело-фиолетовым, затем испаряется частицами. Font.CreateDynamicFontFromOSFont выбирает Gabriola с запасными шрифтами. В момент реплики полосы союзника скрыты, камера удерживает в кадре героя и высокую голову. В GoldenField ночной окружающий свет .075/.095/.145. В форте .36/.39/.43 с локальными огнями. Береговые звуки и чайки затихают по расстоянию от воды. Смерть блокирует движение и действия; еда не возрождает героя. В интерфейсе кнопка новой игры.

`ThreatArt.cs` загружает восемь выбранных новых растровых ресурсов V18 из Resources/Art. Воины идут по атласу 8×3; боевые кадры и крысы — 4×6 с зарегистрированными границами рядов и стопами. Аксолотль: фронт 4×2, профиль и задний вид 4×4. Все новые рисунки выполнены встроенным imagegen; точные промпты, исходники и референсы записаны в `ART-V18.md`. Новые материалы FortStone и Axolotl зарегистрированы в ProjectBuilder, чтобы шейдеры вошли в Windows-сборку. Система ног героя V16 сохранена.

Просмотрены действительные снимки `outputs/Verification/v18-*.png`: карта, внешний и внутренний форт, взрыв, ночная лампа, реплика и частицы, смерть, три полосы бега. Полосы бега сняты с повышенным светом для осмотра рисунка; ночное освещение проверено отдельно. Новые тесты находятся в RuntimeChecks.FortAndHunterChecks и CaptureHunterAnimations. `checkCameraSize` и `checkCameraPosition` работают только в testMode и нужны для общей фотографии форта.

## История V17: огромное поле и карта

По запросу пользователя добавлены процедурное продолжение цветочного поля и карта на M с открытием только посещённых мест. Исходный пляж и цветы сохранены. Поле продолжается бесшовно на север и по бокам: X от -512 до 512, Y до 1024. Это более 650 площадей исходного поля. В продолжении есть цветочные группы, извилистая тропа и ночные светлячки; можно собирать цветы, выбрасывать вещи и ставить костры.

`ProceduralMeadow.cs` содержит `MeadowLayout`, независимую от порядка загрузки генерацию из зерна и потоковую загрузку участков 32×32. В обычном окне загружено до девяти участков; на очень широком экране радиус увеличивается по размеру видимой области. Выгрузка удаляет объекты из общих списков. Сбор запоминается по целочисленному ID цветка: при возвращении собранный цветок не возрождается. Новая игра получает новое зерно, runtime-тесты — фиксированное 74391.

`ExplorationMap.cs` содержит `ExploredRegion` и интерфейс карты. Посещение раскрывает близкие окрестности, а не весь загруженный участок. Старые открытия сохраняются при закрытии карты, выгрузке и переходах. Метки мест тоже скрыты до посещения. Есть масштаб колесом, передвижение зажатой ЛКМ, центрирование на герое и обзор всей области. Посещённые золотое поле, таверна и карманное измерение получают отдельные вкладки. Карта приостанавливает игру и блокирует игровые действия мыши. M, Esc и крестик её закрывают; M больше не отключает звук, звук теперь на F10.

Сохранений между запусками пока нет — исследование, собранные цветы и зерно сбрасываются вместе с новой игрой и остальными системами. В `CoastGame.cs` и `ArcaneCombat.cs` границы движения, выбрасывания и установки костра согласованы с большим полем. Порядок теней учитывает дальнюю координату Y. Шейдер Terrain рисует непрерывную траву и тропу в мировых координатах.

Изображения действительной отрисовки: `outputs/Verification/v17-*.png`. Просмотрены карта у старта, карта золотого поля, дальнее поле, тропа и ночное освещение. Подписи стартовых мест разнесены, чтобы не перекрывать друг друга и значок героя. Система ног V16 сохранена без замены.

## Последнее исправление ног V16

Пользователь прислал скриншоты сломанных ног V15: в профиль ноги сливались, стопы искажались, у пьяницы выглядели как неподвижные полоски. Предыдущая система использовала две одинаковые текстуры `NeutralLegsV15`, сжатые по ширине и изгибаемые шейдером. Её визуальное качество ошибочно оценивалось по техническим тестам.

В V16 `PixelGait.cs` заменён: нижняя часть тела — один цельный спрайт с тазом и обеими ногами. Есть отдельная стойка и восемь нарисованных фаз шага. Четыре новых атласа 5×2 созданы встроенным imagegen и лежат в `Assets/Resources/Art`: `WalkLowerFrontV16.png`, `WalkLowerSideV16.png`, `WalkLowerBackV16.png`, `WalkLowerDiagonalV16.png`. Первый кадр — стойка, затем восемь фаз, последний дублирует стойку. Виды отражаются для правых направлений. Оба колена и стопы нарисованы вместе; программного изгибания отдельных ног больше нет. Края куртки и кисти сохраняются в шейдере при замене нижней части тела. Под мантией мага находятся ноги взрослого масштаба, а не сжатые целые ноги в короткой полоске ниже подола.

Верхняя часть героя всё ещё использует `HeroWalkV14.png` и `HeroLampV14.png`, NPC — `TavernPeopleV14b.png`, `TavernGesturesV14.png`, `MageActorsV14.png`. Эти имена не означают, что запускается старая версия: V16 использует их вместе с новыми нижними позами. Не возвращать активную систему к `NeutralLegsV15`, `NeutralLegMesh` или `PixelWalk`; старые вспомогательные методы/класс в исходниках остались неиспользуемыми.

Сохранены предыдущие исправления: сфера следует за кистью, уменьшена её величина в мире; мелкая золотая трава `GoldenMeadowV15`, расчищенное основание таверны и контактная тень; огонь камина `HearthFireV15` из 16 кадров с плавной интерполяцией и циклом 2,4 секунды. Эти изменения не нужно переделывать без нового запроса.

Снимки действительной отрисовки находятся в `outputs/Verification/v16-walk-filmstrip-*`. Код 0 — герой, 1 — союзный маг, 2 — пьяница; последняя цифра — направление. Слева стойка, затем восемь фаз. Просмотрены все восемь направлений героя, четыре мага и пьяница. Отдельная сводка — `outputs/Verification/visual-review-v16.md`. Если пользователь снова указывает визуальную проблему, оценивать сами снимки и движение, а не доказывать качество количеством пройденных тестов.

## Где менять код

Все пути здесь относительно Unity-проекта `outputs/AshenCoast`:

- `Assets/Scripts/CoastGame.cs`: запуск мира, игрок, перемещение, погода, камера. Скорость героя 3,62 единицы/с, цикл шага зависит от пройденного расстояния.
- `Assets/Scripts/WorldShape.cs`, `FortWorld.cs`, `ThreatArt.cs`, `NightHunter.cs`: контур суши, форт, новые враги и ночной хищник.
- `Assets/Scripts/ProceduralMeadow.cs`: генерация, потоковая загрузка, память собранных цветов, границы большого мира.
- `Assets/Scripts/ExplorationMap.cs`: исследование, скрытие непосещённых мест, карта на M.
- `Assets/Scripts/PixelGait.cs`: активные цельные позы ног, их направление, масштаб и соединение с верхней частью тела.
- `Assets/Scripts/ArtBank.cs`, `ExpansionArt.cs`: загрузка и регистрация спрайтов, масштаб пикселей, привязка кистей.
- `Assets/Shaders/WorldSprite.shader`: пиксельная отрисовка, освещение, сохранение кистей/краёв одежды при замене ног.
- `Assets/Scripts/JourneyWorld.cs`: повозка, золотое поле, таверна, NPC, союзный маг, огонь камина.
- `Assets/Scripts/InventoryModel.cs`, `InventoryUI.cs`: предметы, снаряжение, торговля, крафт, сетки.
- `Assets/Scripts/Combat.cs`, `ArcaneCombat.cs`, `SpatialMagic.cs`, `TentacleField.cs`, `Woodlouse.cs`: оружие, магия, существа.
- `Assets/Scripts/Soundscape.cs`: звуки и музыка.
- `Assets/Editor/ProjectBuilder.cs`: подготовка сцены, импорт ресурсов, сборка Windows и модельные проверки.
- `Assets/Scripts/RuntimeChecks.cs`: проверки работающей Windows-игры, снимки реальной отрисовки.

## Проверка и сборка

Последний выпуск V23: Unity 6000.6.5f1, Windows x64 успешна; 277 модельных и 848 runtime-проверок прошли. Логи последней сборки/проверки: work/unity-build-v23-final.log и work/player-test-v23.log. Отчёты — outputs/build-verification.txt и outputs/Verification/runtime-verification.txt.

Сборка: Unity -batchmode -quit -projectPath "<абсолютный путь проекта>" -executeMethod ProjectBuilder.Build -logFile "<путь к логу>". ASHEN_BUILD_DIRECTORY задаёт промежуточную папку. Проверки: work/run_v23_checks.py запускает -coast-test; игрок теста сам завершает работу. Упаковка проверяет свежесть отчёта, отсутствие ошибок шейдера и SHA256. Запущенную пользователем игру не закрывать.

## Что делать дальше

V23 выпущена. Последний запрос выполнен, незавершённых сборок и тестов нет. Продолжать с тем же Unity-проектом по новым сообщениям пользователя.
