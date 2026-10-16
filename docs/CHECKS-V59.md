# Проверки V59

Unity 6000.6.5f1, Windows x64, приложение 59.0, сетевое приветствие 59.
Проверены все 25 921 клетки навигационной сетки относительно исходных вычислений, совпадение десяти маршрутов у домов, обновление после рубки дерева, смены русла и seed, проходимость трёх мостов и непроходимость воды рядом. Два отдельных процесса с разными исходными seed проверяют приветствие V59, принятие мира ведущего и совпадение маршрутов после подключения.

Замеры отрисовки выполнялись камерой игры в RenderTexture с синхронным чтением одного пикселя, 1440×900, RTX 3050 Laptop. Они исключают оконный интерфейс и ожидание вертикальной синхронизации, поэтому их FPS не следует принимать за FPS пользовательского окна. Для вывода об исправлении используются время расчёта навигации и совпадение результатов. Обычный запуск не включает измеритель или эталонный медленный алгоритм. Испытание двух физических ПК через Radmin не проводилось.

navigation-results.txt: RESULT 18 passed; 0 failed
host-results.txt: RESULT 3 passed; 0 failed
guest-results.txt: RESULT 5 passed; 0 failed

Итоговый измерительный запуск:

GPU NVIDIA GeForce RTX 3050 6GB Laptop GPU; CPU 12th Gen Intel(R) Core(TM) i5-12600H; screen 1440x900
Reference house routes: 128,0ms
Cached house routes: 13,1ms; exact same paths=True
spawn / normal: FPS=369,2 p50=2,7ms p95=3,3ms max=19,7ms slow33=0; navigation=0,0ms/sec calls=0
spawn / AI disabled: FPS=757,9 p50=1,2ms p95=1,6ms max=3,9ms slow33=0; navigation=0,0ms/sec calls=0
village / normal: FPS=353,0 p50=2,8ms p95=3,6ms max=4,9ms slow33=0; navigation=0,1ms/sec calls=2
village / AI disabled: FPS=812,0 p50=1,2ms p95=1,6ms max=4,1ms slow33=0; navigation=0,0ms/sec calls=0
village / rendering disabled: FPS=703,3 p50=1,3ms p95=1,9ms max=11,0ms slow33=0; navigation=6,3ms/sec calls=11
village / normal after warmup: FPS=349,2 p50=2,8ms p95=3,9ms max=10,9ms slow33=0; navigation=6,9ms/sec calls=61
