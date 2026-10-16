# Рисунки V58

Использован встроенный ImageGen. Итоговые PNG с прозрачностью хранятся в `Assets/Resources/Art` и входят в исходный архив. Игра использует эти локальные копии.

- `ChiefHouseV58.png` — дорисованный дом вождя.
- `CrossbowV58.png` — компактный деревянный арбалет на опоре, без сиденья.
- `ArrowV58.png` и `FeatherV58.png` — стрела и перо чайки.
- `LeafBookV58.png` — закрытое и открытое состояния листовой книги.
- `AmazonLowerV58.png` — десять парных поз ног амазонки; девять используются в игре: стойка и восемь фаз шага.
- `OperatorLowerV58.png` — согнутые ноги игрока при управлении арбалетом с земли.

Промпты итоговых изображений:

## Дом

Use case precise-object-edit. Edit target: ONLY the large chief's round thatched log house in the top middle of this sheet. Deliver one standalone complete house sprite, no other sheet objects. Finish the back and both side walls so the entire round building is solid, complete and clearly readable; a generously rounded complete thatch roof with bundled pole cap, carved entrance columns, woven doors open with a dim timber interior floor, complete threshold. Preserve exact overall original medieval hand-drawn pixel art style and front three-quarter top-down 2.5D RPG perspective, golden straw and dark brown timber and flower bunches. No missing edges or cut off roof, entire building including base visible with padding. Actual transparent background, remove brown backdrop, no ground, no grass, no people, no UI/text.

Референс: AmazonVillageV46.png.

## Арбалет

Use case precise-object-edit. Edit target: this stationary crossbow sprite. Remove the entire seat/chair and its support on the left, leaving only the crossbow with a short stock, winding crank and its compact wooden swivel pedestal. Keep the detailed pixel art wood, iron, rope style, orientation pointing right, lighting and true transparent background. Make pedestal modest and compact; no large siege platform. Complete clean silhouette fully visible. No seat, no chair, no lettering.

Первый вариант создан по предоставленному пользователем референсу деревянного арбалета; затем сиденье удалено по его уточнению.

## Книга

Use case stylized-concept. Asset: two states of ONE magical leaf book in a horizontal sprite sheet with two equal square cells and wide clear gap. Detailed medieval pixel art for top down 2.5D RPG Ashen Coast. Reference shows the plant knot writing. LEFT cell: CLOSED book made from exactly two overlapping broad green leaves like covers, compact horizontal silhouette, thin plant cords tucked inside with a few small dangling knots. RIGHT cell: OPEN book, two leaves spread across the top like two pages, many long hanging braided plant cords whose different knots are its writing. Open silhouette narrow and tall, width:height approximately 2:3. Four subtly distinguished clusters of knotted cords, empty book with no magic symbols. Bright but natural emerald green leaf veins, dark forest green braided fibres, gold green highlights. Whole objects isolated and fully visible. Real transparent background, no floor/shadow, no text, no labels, no boxes, no extra items. Clear separation and aligned bottoms. Closed book much shorter than open one.

Референс: изображение листовой книги пользователя.

## Стрела

Use case stylized-concept. One inventory sprite of a single medieval wooden arrow, point up, long straight wood shaft, compact grey iron arrowhead at top and two small white gull feather vanes at bottom. Detailed dark medieval pixel art, brown wood, charcoal outline, clean highlights. Narrow vertical silhouette width:height 1:5. Entire arrow visible in centered tall canvas with padding. Actual transparent background, no backdrop or shadows, no text/UI, no other objects.

## Перо

Use case stylized-concept. Asset: a single white and soft grey seagull feather for a medieval RPG inventory. Detailed hand-drawn pixel art, lightly curved quill, cream white barbs with charcoal grey shadow and subtle warm highlights. Diagonal feather centered and fully visible with generous padding. Actual transparent background, no floor or other object, no text, no UI.

## Ноги амазонок

Use case precise-object-edit. This is an animation sheet, STRICTLY preserve all TEN leg pair poses, their alternating planted feet, knee bends, silhouettes, exact order, scale, locations, baseline and transparent gaps: 5 columns x 2 rows. Change only the clothing: turn the blue trouser area above the brown boots into slender female Amazon bare thighs with warm natural skin, a short dark forest-green ragged leather skirt and brown waist belt at the top. Keep the brown knee-high boots and their soles in EXACTLY the same orientations and leg positions as the input. Keep natural anatomy and hand-painted medieval pixel art. Do not change the gait, do not replace all frames by the same stance. No torso, no head, no arms, no text, no labels, genuine transparent background. Ten complete PAIRS of legs, all side view facing left as reference.

Референс: WalkLowerSideV16.png. Направление в игре задаётся отражением всего парного кадра.

## Поза за арбалетом

Use case precise-object-edit / new animation pose from reference. Create ONE isolated paired lower-body sprite matching the reference's dark blue trousers and brown leather boots, same detailed medieval hand-drawn pixel art. SIDE VIEW FACING LEFT. The person is seated low or kneeling on the ground behind a small stationary crossbow: both knees bent forward and both boots tucked under, relaxed natural crouch. Draw from the waist belt down only, no torso, no hands, no head. Two anatomically complete legs with natural overlapping folded knees, clear trouser seams and boot soles. Compact horizontal shape, width slightly greater than height. No chair, no stool, no crossbow, no ground, no shadows. Actual transparent background. One clean complete sprite, no sheet or labels. Consistent brown boots and dark charcoal blue cloth as input.

Референс: WalkLowerSideV16.png.
