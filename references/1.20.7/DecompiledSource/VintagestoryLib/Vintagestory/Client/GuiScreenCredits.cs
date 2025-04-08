using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.Client.NoObf;

namespace Vintagestory.Client;

public class GuiScreenCredits : GuiScreen
{
	private string credits = "<font color='#AAE0CFBB'>{0}</font>\r\n\r\n<font color='#99E0CFBB' size='22'>Active Team Members</font>\r\nLo-Phi <font color='#AAE0CFBB'>(Music)</font>\r\nLuke Jeffrey <font color='#AAE0CFBB'>(Story, Animation, Game design, Production)</font>\r\nSaraty <font color='#AAE0CFBB'>(Textures, Models, Game design)</font>\r\nBalduranne <font color='#AAE0CFBB'>(Textures, Models, Animation)</font>\r\nRedRam <font color='#AAE0CFBB'>(Textures, Models, Game design, Moderator)</font>\r\nradfast <font color='#AAE0CFBB'>(Programming)</font>\r\nElvas <font color='#AAE0CFBB'>(Building/Level design)</font>\r\nJasper <font color='#AAE0CFBB'>(Building/Level design)</font>\r\nVallen <font color='#AAE0CFBB'>(Customer support, Community Manager)</font>\r\nDuargra <font color='#AAE0CFBB'>(Concept art)</font>\r\nManuel Dielacher <font color='#AAE0CFBB'>(Programming)</font>\r\nAmbroise Hennebelle <font color='#AAE0CFBB'>(Animation)</font>\r\nTyron <font color='#AAE0CFBB'>(Lead, Web/Game/Engine programming, everything else)</font>\r\n\r\n<font color='#99E0CFBB' size='22'>Other Current and Former Contributors</font>\r\nRoman Polikarpov (Maltiez) <font color='#AAE0CFBB'>(Bug fixing)</font>\r\nAranai Ra <font color='#AAE0CFBB'>(Customer support)</font>\r\nRythillian <font color='#AAE0CFBB'>(Sound design)</font>\r\nAndrew Kozma <font color='#AAE0CFBB'>(Writing)</font>\r\nElsa Lamoureux <font color='#AAE0CFBB'>(Animation)</font>\r\nLucie Champion <font color='#AAE0CFBB'>(Animation)</font>\r\nNathan Gilabert <font color='#AAE0CFBB'>(Animation)</font>\r\nMaxime Bidaut <font color='#AAE0CFBB'>(Animation)</font>\r\nAndrey Vinogradov <font color='#AAE0CFBB'>(Hurdy Gurdy Music Track)</font>\r\nnat <font color='#AAE0CFBB'>(Mod api documentation)</font>\r\nArkaik <font color='#AAE0CFBB'>(Textures)</font>\r\nkashcah <font color='#AAE0CFBB'>(Customer support)</font>\r\nvectorslayers <font color='#AAE0CFBB'>(Vector art)</font>\r\nColter Hendrickson <font color='#AAE0CFBB'>(Sound design)</font>\r\nK.jpq <font color='#AAE0CFBB'>(Worldgen Noise)</font>\r\nKrzysztof Zięba <font color='#AAE0CFBB'>(Writing)</font>\r\nSaria Akineko <font color='#AAE0CFBB'>(Writing)</font>\r\nChase Myers <font color='#AAE0CFBB'>(Customer support)</font>\r\nArkan <font color='#AAE0CFBB'>(Building/Level design)</font>\r\nNormeda <font color='#AAE0CFBB'>(Concept art)</font>\r\nVilderos <font color='#AAE0CFBB'>(Concept art, Writing)</font>\r\nJessie Rogers <font color='#AAE0CFBB'>(Concept art)</font>\r\nDArkHekRoMaNT <font color='#AAE0CFBB'>(Compatibility lib)</font>\r\nCynthal <font color='#AAE0CFBB'>(Slanted shingle roofs)</font>\r\nMilo Christiansen <font color='#AAE0CFBB'>(Vertical slabs)</font>\r\nAshantin <font color='#AAE0CFBB'>(Video tutorials, Wiki)</font>\r\nNovocain <font color='#AAE0CFBB'>(Code contributions)</font>\r\nSkodone <font color='#AAE0CFBB'>(Concept art)</font>\r\ncopygirl <font color='#AAE0CFBB'>(Mod API, Software consulting)</font>\r\nTatazaki <font color='#AAE0CFBB'>(Localization, Crash reporter, Mod API docs)</font>\r\nCreativeMD <font color='#AAE0CFBB'>(Mod API, Mod API docs)</font>\r\nCynthia Revström <font color='#AAE0CFBB'>(Security)</font>\r\nMonsterfish_  <font color='#AAE0CFBB'>(Vines)</font>\r\nFlorian Graier <font color='#AAE0CFBB'>(Logo and UI concept)</font>\r\nJ. W. Bjerk <font color='#AAE0CFBB'>(eleazzaar)</font> <font color='#AAE0CFBB'>(One Flower, generic wood texture)</font>\r\nDevon Thomson <font color='#AAE0CFBB'>(Tree bark textures)</font>\r\nJ. Thomas <font color='#AAE0CFBB'>(Herb textures)</font>\r\nSamuel Legnér <font color='#AAE0CFBB'>(Textures, Models)</font>\r\nJosh Chappelle <font color='#AAE0CFBB'>(Gameplay programming)</font>\r\nIvan Farina <font color='#AAE0CFBB'>(Building/Level design)</font>\r\n\r\n<font color='#99E0CFBB' size='22'>Discord Moderators</font>\r\nAphelion\r\nDArkHekRoMaNT\r\nRorax\r\nVinter Nacht\r\nZaldaryon\r\n\r\n<font color='#99E0CFBB' size='22'>Wiki Contributors</font>\r\nAkella\r\nAlex Brainhow\r\nAlienijena10\r\nAtteAgus\r\nBob\r\nChase Myers\r\nCraluminum\r\nCreativeMD\r\nDaretmavi\r\nDarce\r\nDarnuria\r\ndegradka\r\nDmitrijs Oginskis\r\nDocek\r\nEleli\r\nEreketh\r\nErop\r\nFelix Hüsam\r\nFibojoly\r\nFuzzyBot\r\ngattsuru\r\nGravydigger\r\nGuillermo Ruiz\r\nHayden Davenport\r\nJake kgz\r\nJo\r\nJohan oldsen\r\nJuan Diego Arce\r\nJulius\r\nJustOmi\r\nKengo700\r\nlaw\r\nLpiro\r\nMaciej Piechocki\r\nMard\r\nMinecrafter\r\nNiclas\r\nPixelCat\r\nRichard Böddeker\r\nRodolfo Clash\r\nRomain Peyrat\r\nSana\r\nSaria Akineko\r\nSteelWool\r\nPaul\r\nT.Read\r\nTenebrisTerra\r\nToastyDerek\r\nTowelcat\r\nTyron\r\nValentin\r\nveerserif\r\nVeryGoodDog\r\nVinLake\r\nXandoria\r\nXurxo Martínez Ferreira\r\nYota\r\nВячеслав\r\nТимофей\r\n\r\n<font color='#99E0CFBB' size='22'>Major Translators</font>\r\ncatzdutz <font color='#AAE0CFBB'> (Afrikaans) </font>\r\nعبدالرحمن (dhomrrad) <font color='#AAE0CFBB'> (Arabic) </font>\r\nAArelik <font color='#AAE0CFBB'> (Arabic) </font>\r\nNil Badia <font color='#AAE0CFBB'> (Catalan) </font>\r\n42yeah <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nFuge John <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nKerocate_军喵 <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nkousakak <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nlaw_4x <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nPopeVI <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\nxingye <font color='#AAE0CFBB'> (Chinese Simplified) </font>\r\n石偉呈 (tentail10) <font color='#AAE0CFBB'> (Chinese Traditional) </font>\r\n堯KEKEKE YAW <font color='#AAE0CFBB'> (Chinese Traditional) </font>\r\n艾神 (ns954111) <font color='#AAE0CFBB'> (Chinese Traditional) </font>\r\nLawrence Tsung <font color='#AAE0CFBB'> (Chinese Traditional) </font>\r\nAttie (Athlonx8) <font color='#AAE0CFBB'> (Croatian) </font>\r\nTed Alderson <font color='#AAE0CFBB'> (Croatian) </font>\r\nDevBodlacek (Hrbitovnik) <font color='#AAE0CFBB'> (Czech) </font>\r\nHaradiann <font color='#AAE0CFBB'> (Czech) </font>\r\nIslacrusez <font color='#AAE0CFBB'> (Czech) </font>\r\nJiří (SpawnLussCz) <font color='#AAE0CFBB'> (Czech) </font>\r\nMichal Reindl <font color='#AAE0CFBB'> (Czech) </font>\r\nodyseus156 <font color='#AAE0CFBB'> (Czech) </font>\r\nPetr Machát (viz.machat) <font color='#AAE0CFBB'> (Czech) </font>\r\nMartinKoehl <font color='#AAE0CFBB'> (Czech) </font>\r\nMatěj Jakubec <font color='#AAE0CFBB'> (Czech) </font>\r\nSpesMonkeh <font color='#AAE0CFBB'> (Danish) </font>\r\nTanny Lund Deutsch-Lauritsen (Drakxter) <font color='#AAE0CFBB'> (Danish) </font>\r\nAlteOgre <font color='#AAE0CFBB'> (Dutch) </font>\r\nMeorin <font color='#AAE0CFBB'> (Dutch) </font>\r\nMink69 <font color='#AAE0CFBB'> (Dutch) </font>\r\nCassandra Lone (Astronastra) (Amastelo) <font color='#AAE0CFBB'> (Esperanto) </font>\r\nMekhanicusGaming <font color='#AAE0CFBB'> (Esperanto) </font>\r\nProtoManly <font color='#AAE0CFBB'> (Esperanto) </font>\r\nRolfMeles <font color='#AAE0CFBB'> (Esperanto) </font>\r\nSchaumschatten <font color='#AAE0CFBB'> (Esperanto) </font>\r\nainomaria <font color='#AAE0CFBB'> (Finnish) </font>\r\nlisunki <font color='#AAE0CFBB'> (Finnish) </font>\r\nSadelite <font color='#AAE0CFBB'> (Finnish) </font>\r\nAilin78 <font color='#AAE0CFBB'> (French) </font>\r\nAledark <font color='#AAE0CFBB'> (French) </font>\r\nanthony.maayer <font color='#AAE0CFBB'> (French) </font>\r\nARandwulf <font color='#AAE0CFBB'> (French) </font>\r\nBenoit67 <font color='#AAE0CFBB'> (French) </font>\r\nbondnokey <font color='#AAE0CFBB'> (French) </font>\r\nChronodrax <font color='#AAE0CFBB'> (French) </font>\r\nCyannoushco <font color='#AAE0CFBB'> (French) </font>\r\nDrakker <font color='#AAE0CFBB'> (French) </font>\r\nernest33 <font color='#AAE0CFBB'> (French) </font>\r\njbs04 <font color='#AAE0CFBB'> (French) </font>\r\nJean Charles Passard <font color='#AAE0CFBB'> (French) </font>\r\nLiberodark <font color='#AAE0CFBB'> (French) </font>\r\nlolmamgamefr <font color='#AAE0CFBB'> (French) </font>\r\nMascettifabien <font color='#AAE0CFBB'> (French) </font>\r\nMokradin <font color='#AAE0CFBB'> (French) </font>\r\nPanPraescribens <font color='#AAE0CFBB'> (French) </font>\r\nRoulito <font color='#AAE0CFBB'> (French) </font>\r\nSana <font color='#AAE0CFBB'> (French) </font>\r\nTriForMine <font color='#AAE0CFBB'> (French) </font>\r\nXandoria <font color='#AAE0CFBB'> (French) </font>\r\nA. Spenkuch <font color='#AAE0CFBB'> (German) </font>\r\nAki <font color='#AAE0CFBB'> (German) </font>\r\nAlpha Ray <font color='#AAE0CFBB'> (German) </font>\r\nAngelnoir <font color='#AAE0CFBB'> (German) </font>\r\nErik3003 <font color='#AAE0CFBB'> (German) </font>\r\nChemie ist cool <font color='#AAE0CFBB'> (German) </font>\r\nChristian Jog <font color='#AAE0CFBB'> (German) </font>\r\nchyper <font color='#AAE0CFBB'> (German) </font>\r\nCreativeMD <font color='#AAE0CFBB'> (German) </font>\r\nDaniel <font color='#AAE0CFBB'> (German) </font>\r\nDasPrinzip <font color='#AAE0CFBB'> (German) </font>\r\ndeadzombie <font color='#AAE0CFBB'> (German) </font>\r\ndKrisch <font color='#AAE0CFBB'> (German) </font>\r\nDrKrieger <font color='#AAE0CFBB'> (German) </font>\r\nFulgen301 <font color='#AAE0CFBB'> (German) </font>\r\nHaret[UGC] <font color='#AAE0CFBB'> (German) </font>\r\nJoseph <font color='#AAE0CFBB'> (German) </font>\r\nkerbezena <font color='#AAE0CFBB'> (German) </font>\r\nManuel Pietsch (dauerzockerhoch2) <font color='#AAE0CFBB'> (German) </font>\r\nminelpphynix2 <font color='#AAE0CFBB'> (German) </font>\r\nMondarian <font color='#AAE0CFBB'> (German) </font>\r\nMonstanner <font color='#AAE0CFBB'> (German) </font>\r\nNiclas S. <font color='#AAE0CFBB'> (German) </font>\r\nRai'jin <font color='#AAE0CFBB'> (German) </font>\r\nRolfMeles <font color='#AAE0CFBB'> (German) </font>\r\nSaru <font color='#AAE0CFBB'> (German) </font>\r\nskol <font color='#AAE0CFBB'> (German) </font>\r\nSkruwel <font color='#AAE0CFBB'> (German) </font>\r\nTaija <font color='#AAE0CFBB'> (German) </font>\r\nTels <font color='#AAE0CFBB'> (German) </font>\r\nVoxelman <font color='#AAE0CFBB'> (German) </font>\r\nxBeathovenx <font color='#AAE0CFBB'> (German) </font>\r\nVeikko Nitzsche <font color='#AAE0CFBB'> (German) </font>\r\nVintage_LP <font color='#AAE0CFBB'> (German) </font>\r\nWilhelm Dacson <font color='#AAE0CFBB'> (German) </font>\r\nSnossoz <font color='#AAE0CFBB'> (German) </font>\r\nteddy vadmad <font color='#AAE0CFBB'> (Hebrew) </font>\r\nBamboo2 <font color='#AAE0CFBB'> (Hungarian) </font>\r\nMario_D <font color='#AAE0CFBB'> (Hungarian) </font>\r\nSmilei0 <font color='#AAE0CFBB'> (Hungarian) </font>\r\nXKot105 <font color='#AAE0CFBB'> (Hungarian) </font>\r\nMýraMidnight <font color='#AAE0CFBB'> (Icelandic) </font>\r\nAlipheese Fateburn <font color='#AAE0CFBB'> (Italian) </font>\r\nDaveDevil <font color='#AAE0CFBB'> (Italian) </font>\r\nFranciscono.enry <font color='#AAE0CFBB'> (Italian) </font>\r\nMinecraftPlayerOnline01 <font color='#AAE0CFBB'> (Italian) </font>\r\nMirko Di Bello <font color='#AAE0CFBB'> (Italian) </font>\r\nNino Segreto <font color='#AAE0CFBB'> (Italian) </font>\r\nSam-pie <font color='#AAE0CFBB'> (Italian) </font>\r\nStefanoG01 <font color='#AAE0CFBB'> (Italian) </font>\r\ntowersf <font color='#AAE0CFBB'> (Italian) </font>\r\nx_Yota_x <font color='#AAE0CFBB'> (Italian) </font>\r\nAlisAlia <font color='#AAE0CFBB'> (Japanese) </font>\r\nkengo700 <font color='#AAE0CFBB'> (Japanese) </font>\r\nmacoto_hino <font color='#AAE0CFBB'> (Japanese) </font>\r\nNyuhnyash <font color='#AAE0CFBB'> (Japanese) </font>\r\nRikeiR (2007tera) <font color='#AAE0CFBB'> (Japanese) </font>\r\nsiroru <font color='#AAE0CFBB'> (Japanese) </font>\r\nWINCHaN <font color='#AAE0CFBB'> (Japanese) </font>\r\n전인성 (Doto_Core) <font color='#AAE0CFBB'> (Korean) </font>\r\n사용자 (gelr) <font color='#AAE0CFBB'> (Korean) </font>\r\n황두현 (hdhyun94) <font color='#AAE0CFBB'> (Korean) </font>\r\n하츠세이즈나 (minku0430) <font color='#AAE0CFBB'> (Korean) </font>\r\n박미르 (mireu) <font color='#AAE0CFBB'> (Korean) </font>\r\n2hh8899 <font color='#AAE0CFBB'> (Korean) </font>\r\nhogaeng (meister.maestro.b.c) <font color='#AAE0CFBB'> (Korean) </font>\r\nJeyeol (soncheiol) <font color='#AAE0CFBB'> (Korean) </font>\r\nJuris <font color='#AAE0CFBB'> (Latvian) </font>\r\nTBNRregs <font color='#AAE0CFBB'> (Latvian) </font>\r\nmarks27 <font color='#AAE0CFBB'> (Latvian) </font>\r\nJillieDaw <font color='#AAE0CFBB'> (Lithuanian) </font>\r\npirikuo <font color='#AAE0CFBB'> (Lithuanian) </font>\r\nMrBone <font color='#AAE0CFBB'> (Norwegian) </font>\r\nAcheon72P <font color='#AAE0CFBB'> (Polish) </font>\r\nAdam_eM <font color='#AAE0CFBB'> (Polish) </font>\r\nAleksandra Pojawa <font color='#AAE0CFBB'> (Polish) </font>\r\nAndrzej.W <font color='#AAE0CFBB'> (Polish) </font>\r\nArczi08 <font color='#AAE0CFBB'> (Polish) </font>\r\nDawid Rogalski <font color='#AAE0CFBB'> (Polish) </font>\r\nFlayedCrusader <font color='#AAE0CFBB'> (Polish) </font>\r\nGabriel Kaszewski <font color='#AAE0CFBB'> (Polish) </font>\r\nK0cur <font color='#AAE0CFBB'> (Polish) </font>\r\nKaperios <font color='#AAE0CFBB'> (Polish) </font>\r\nkchmiel <font color='#AAE0CFBB'> (Polish) </font>\r\nkuballaszymanski <font color='#AAE0CFBB'> (Polish) </font>\r\nkrzemcis <font color='#AAE0CFBB'> (Polish) </font>\r\nLina_Hinohana <font color='#AAE0CFBB'> (Polish) </font>\r\nMorgus <font color='#AAE0CFBB'> (Polish) </font>\r\nMefistos_ <font color='#AAE0CFBB'> (Polish) </font>\r\nMrGREGhimself <font color='#AAE0CFBB'> (Polish) </font>\r\nnheve <font color='#AAE0CFBB'> (Polish) </font>\r\nPaweł Górny <font color='#AAE0CFBB'> (Polish) </font>\r\nPaweł (Kaznodziej) <font color='#AAE0CFBB'> (Polish) </font>\r\nqexow <font color='#AAE0CFBB'> (Polish) </font>\r\nQWERTYQ1234 <font color='#AAE0CFBB'> (Polish) </font>\r\nRadiden <font color='#AAE0CFBB'> (Polish) </font>\r\nscorbiot <font color='#AAE0CFBB'> (Polish) </font>\r\nSyllphid <font color='#AAE0CFBB'> (Polish) </font>\r\nTheCrossGirl <font color='#AAE0CFBB'> (Polish) </font>\r\nTheK0cur <font color='#AAE0CFBB'> (Polish) </font>\r\nWuochiDemon <font color='#AAE0CFBB'> (Polish) </font>\r\nLorenzoBane <font color='#AAE0CFBB'> (Portuguese) </font>\r\nMaus_Gamer <font color='#AAE0CFBB'> (Portuguese) </font>\r\nMichaloid <font color='#AAE0CFBB'> (Portuguese) </font>\r\nOkaîbaʻe (Okaibae) <font color='#AAE0CFBB'> (Portuguese) </font>\r\nRuyeex <font color='#AAE0CFBB'> (Portuguese) </font>\r\nxX||Doge_King||Xx <font color='#AAE0CFBB'> (Portuguese) </font>\r\nDracomancer <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nLucas (LucasR) <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nnasiro <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nplaic_ <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nvictor araújo (BoletoAgiota) <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nyanazake <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nZaldaryon <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nZilchPhoenix <font color='#AAE0CFBB'> (Portuguese, Brazilian) </font>\r\nFeykro Sil <font color='#AAE0CFBB'> (Romanian) </font>\r\nfruitshakes <font color='#AAE0CFBB'> (Romanian) </font>\r\ngazelutza <font color='#AAE0CFBB'> (Romanian) </font>\r\nLefter Mădălin <font color='#AAE0CFBB'> (Romanian) </font>\r\nLumea lui iCiuncio (lumea_iciuncio) <font color='#AAE0CFBB'> (Romanian) </font>\r\nMedecube <font color='#AAE0CFBB'> (Romanian) </font>\r\nMKDelta (VoyagerTrinity) <font color='#AAE0CFBB'> (Romanian) </font>\r\nSile Bandit (SileBandit) <font color='#AAE0CFBB'> (Romanian) </font>\r\nбарбитурный (Giris_) <font color='#AAE0CFBB'> (Russian) </font>\r\nСухофрулт (Syhofrukt) <font color='#AAE0CFBB'> (Russian) </font>\r\nЕвгений Кольцов (scheugen86) <font color='#AAE0CFBB'> (Russian) </font>\r\nЕвгений Кольцов (schmarotzerltd) <font color='#AAE0CFBB'> (Russian) </font>\r\n1aarklight1 <font color='#AAE0CFBB'> (Russian) </font>\r\nbit62042 <font color='#AAE0CFBB'> (Russian) </font>\r\nD <font color='#AAE0CFBB'> (Russian) </font>\r\nDaniil1254 <font color='#AAE0CFBB'> (Russian) </font>\r\nDArkHekRoMaNT <font color='#AAE0CFBB'> (Russian) </font>\r\ndias nurlanuli <font color='#AAE0CFBB'> (Russian) </font>\r\nErfarenort <font color='#AAE0CFBB'> (Russian) </font>\r\nfrompix <font color='#AAE0CFBB'> (Russian) </font>\r\nGugster <font color='#AAE0CFBB'> (Russian) </font>\r\nHan_Salo <font color='#AAE0CFBB'> (Russian) </font>\r\ninsectdesign111 <font color='#AAE0CFBB'> (Russian) </font>\r\nJavapony <font color='#AAE0CFBB'> (Russian) </font>\r\nLiv Frog <font color='#AAE0CFBB'> (Russian) </font>\r\nLow3 <font color='#AAE0CFBB'> (Russian) </font>\r\nMakiNok <font color='#AAE0CFBB'> (Russian) </font>\r\nmax sobanin <font color='#AAE0CFBB'> (Russian) </font>\r\nMechanicusGaming <font color='#AAE0CFBB'> (Russian) </font>\r\nMinecrafter <font color='#AAE0CFBB'> (Russian) </font>\r\nMirotworez <font color='#AAE0CFBB'> (Russian) </font>\r\nmrlobaker <font color='#AAE0CFBB'> (Russian) </font>\r\nNyuhnyash <font color='#AAE0CFBB'> (Russian) </font>\r\np.taranov <font color='#AAE0CFBB'> (Russian) </font>\r\npseudokot <font color='#AAE0CFBB'> (Russian) </font>\r\nrasperryice <font color='#AAE0CFBB'> (Russian) </font>\r\nscepsis <font color='#AAE0CFBB'> (Russian) </font>\r\nTed Alderson <font color='#AAE0CFBB'> (Russian) </font>\r\nTimBack <font color='#AAE0CFBB'> (Russian) </font>\r\nVedansulim <font color='#AAE0CFBB'> (Russian) </font>\r\nzybillo <font color='#AAE0CFBB'> (Russian) </font>\r\ndegradka <font color='#AAE0CFBB'> (Serbian Cyrillic) </font>\r\nMiljan Trajkovic <font color='#AAE0CFBB'> (Serbian Cyrillic) </font>\r\nmNikola <font color='#AAE0CFBB'> (Serbian Cyrillic) </font>\r\nTed Alderson <font color='#AAE0CFBB'> (Serbian Cyrillic) </font>\r\ndaretmavi <font color='#AAE0CFBB'> (Slovakian) </font>\r\nLiv Frog <font color='#AAE0CFBB'> (Slovakian) </font>\r\nDaRKKoNNaN <font color='#AAE0CFBB'> (Spanish) </font>\r\nEliasS bianchi (bianchi.elias) <font color='#AAE0CFBB'> (Spanish) </font>\r\nGrecoPlay <font color='#AAE0CFBB'> (Spanish) </font>\r\nHola Adios <font color='#AAE0CFBB'> (Spanish) </font>\r\nJefferzoonn (gatocosmico231) <font color='#AAE0CFBB'> (Spanish) </font>\r\nJonh Black <font color='#AAE0CFBB'> (Spanish) </font>\r\nJuan Diego Arce (Darce) <font color='#AAE0CFBB'> (Spanish) </font>\r\nJuan Pablo Ossa Zapata <font color='#AAE0CFBB'> (Spanish) </font>\r\nLuis B <font color='#AAE0CFBB'> (Spanish) </font>\r\nmatias del villar <font color='#AAE0CFBB'> (Spanish) </font>\r\nPolRP <font color='#AAE0CFBB'> (Spanish) </font>\r\nRicardoMM00 <font color='#AAE0CFBB'> (Spanish) </font>\r\nRoberto Vela Vargas <font color='#AAE0CFBB'> (Spanish) </font>\r\nRIXER OMAR <font color='#AAE0CFBB'> (Spanish) </font>\r\nSebastián Ré (Squoshy) <font color='#AAE0CFBB'> (Spanish) </font>\r\nSebastiàn Ruiz <font color='#AAE0CFBB'> (Spanish) </font>\r\nsocramazibi <font color='#AAE0CFBB'> (Spanish) </font>\r\nSteelWool <font color='#AAE0CFBB'> (Spanish) </font>\r\nXurxo Martínez Ferreira (xurxomf) <font color='#AAE0CFBB'> (Spanish) </font>\r\nZulfBracket <font color='#AAE0CFBB'> (Spanish) </font>\r\nCrisDan <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nedualdo (eduardojbrachoff) <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nEleiber (eleiber) <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nEliasS bianchi <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nFrancisCuarto <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nGrecoPlay <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nJefferson Flores <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nJuan Diego Arce (Darce) <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nluis franco <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nMario Erick Barrera <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nNahuel-Campos <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nNathycore Weird <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nRicherd Guzmàn <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nSeshuOM <font color='#AAE0CFBB'> (Spanish, Latin America) </font>\r\nhasselkanin <font color='#AAE0CFBB'> (Swedish) </font>\r\nLutowski <font color='#AAE0CFBB'> (Swedish) </font>\r\nVilderos <font color='#AAE0CFBB'> (Swedish) </font>\r\nmunkeawtoast <font color='#AAE0CFBB'> (Thai) </font>\r\njan Aselen <font color='#AAE0CFBB'> (Toki Pona) </font>\r\njan Pawasi <font color='#AAE0CFBB'> (Toki Pona) </font>\r\nplutonicHumanoid <font color='#AAE0CFBB'> (Toki Pona) </font>\r\nWreckstation <font color='#AAE0CFBB'> (Toki Pona) </font>\r\nCossack <font color='#AAE0CFBB'> (Turkish) </font>\r\nMadGamer <font color='#AAE0CFBB'> (Turkish) </font>\r\nÖmer Faruk Saǧlam <font color='#AAE0CFBB'> (Turkish) </font>\r\nŞerwan Özel <font color='#AAE0CFBB'> (Turkish) </font>\r\nYusuf Kerem Kahraman <font color='#AAE0CFBB'> (Turkish) </font>\r\nВладислав Вишневський (Vyshnevskyi) <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nОлександр Козак (kozakoleksandr97) <font color='#AAE0CFBB'> (Ukrainian) </font>\r\n_epic_fish_ <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nAZsSPC <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nDearFox <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nDmytro Vystoropskyi <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nHan_Salo <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nkozakoleksandr97 <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nMaxim Bykov <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nMarsen Bykov <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nMinecrafter <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nMykola Sheredko <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nOlexandr Nesterenko <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nOmi <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nSerhii Chekun (Chekushka) <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nSkif_97 <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nTapio (Tapio_tio) <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nYaroslav Popov <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nYaroslav Yarylo <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nYevhenii Vuksta <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nVladyslav Dudas (Han_Salo) <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nYevhenii Vuksta <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nzirka <font color='#AAE0CFBB'> (Ukrainian) </font>\r\nAndyVipEvil <font color='#AAE0CFBB'> (Vietnamese) </font>\r\n\r\nMore credits are in the credits.txt file\r\n";

	public GuiScreenCredits(ScreenManager screenManager, GuiScreen parentScreen)
		: base(screenManager, parentScreen)
	{
		ShowMainMenu = true;
		InitGui();
		screenManager.GamePlatform.WindowResized += delegate
		{
			invalidate();
		};
		ClientSettings.Inst.AddWatcher<float>("guiScale", delegate
		{
			invalidate();
		});
	}

	private void invalidate()
	{
		if (base.IsOpened)
		{
			InitGui();
		}
		else
		{
			ScreenManager.GuiComposers.Dispose("mainmenu-credits");
		}
	}

	private void InitGui()
	{
		int windowHeight = ScreenManager.GamePlatform.WindowSize.Height;
		int windowWidth = ScreenManager.GamePlatform.WindowSize.Width;
		double width = Math.Max(400.0, (double)windowWidth * 0.5) / (double)ClientSettings.GUIScale;
		ElementBounds textBounds = ElementBounds.Fixed(0.0, 0.0, width, (float)Math.Max(300, windowHeight) / ClientSettings.GUIScale - 175f);
		ElementBounds titleBounds = ElementBounds.Fixed(EnumDialogArea.LeftTop, 0.0, 0.0, 690.0, 30.0);
		ElementBounds insetBounds = textBounds.ForkBoundingParent(5.0, 5.0, 5.0, 5.0).FixedUnder(titleBounds);
		ElementBounds clippingBounds = textBounds.CopyOffsetedSibling();
		ElementBounds scrollbarBounds = ElementStdBounds.VerticalScrollbar(insetBounds);
		ElementComposer = dialogBase("mainmenu-credits").AddStaticText(Lang.Get("credits-epicaddress"), CairoFont.WhiteSmallishText(), titleBounds).AddInset(insetBounds).AddVerticalScrollbar(OnNewScrollbarvalue, scrollbarBounds, "scrollbar")
			.BeginClip(clippingBounds)
			.AddRichtext(string.Format(credits, Lang.Get("credits-title")), CairoFont.WhiteSmallishText().WithLineHeightMultiplier(1.15), textBounds, null, "credits")
			.EndClip()
			.EndChildElements()
			.Compose();
		clippingBounds.CalcWorldBounds();
		ElementComposer.GetScrollbar("scrollbar").SetHeights((float)clippingBounds.fixedHeight, (float)textBounds.fixedHeight);
	}

	private void OnNewScrollbarvalue(float value)
	{
		ElementBounds bounds = ElementComposer.GetRichtext("credits").Bounds;
		bounds.fixedY = 10f - value;
		bounds.CalcWorldBounds();
	}
}
