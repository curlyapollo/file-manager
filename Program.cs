using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;

namespace peer3
{
    /// <summary>
    /// Класс, содержащий программу.
    /// </summary>
    class Program
    {
        
        /// <summary>
        /// Путь по умолчанию.
        /// </summary>
        static string defaultPath = "";

        /// <summary>
        /// Точка входа.
        /// </summary>
        static void Main()
        {
            string yes;
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(Instruction());
                MiddleOfWork();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Твой путь по умолчанию на данный момент: {defaultPath}");
                Console.WriteLine(@"Славно потрудились! Если хочешь поработать ещё, напиши 'да'. 
Если же тебе надоело, то введи что-либо другое.");
                yes = Console.ReadLine();
            } while (yes == "да");

            Console.WriteLine("Хорошего дня! Работа окончена.");
        }

        /// <summary>
        /// Инструкция по использованию менеджера.
        /// </summary>
        /// <returns>Инструкция</returns>
        static string Instruction()
        {
            string instruction;
            instruction = @"____________________________________________________________________________________________
RULES!!!!!!!
Привет, мой дорогой сокурсник! Ну или моя прекрасная сокурсница. 
Представляю тебе свой файловый менеджер! Правила просты: ты выбираешь, какие действия выполнять с файловой системой. 
Тебе будет предложено 10 опций: 
1. просмотр списка дисков компьютера и выбор диска;
2. переход в другую директорию (выбор папки);
3. просмотр списка файлов в директории;
4. вывод содержимого текстового файла в консоль в выбранной
пользователем кодировке (предоставляется не менее трех вариантов, в том числе UTF-8);
5. копирование файла;
6. перемещение файла в выбранную пользователем директорию;
7. удаление файла;
8. создание простого текстового файла в выбранной пользователем кодировке (в том числе UTF-8);
9. конкатенация содержимого двух или более текстовых файлов и вывод результата в консоль в кодировке UTF-8;
10. вывод всех файлов в текущей директории (или ещё всех её поддиректориях) по заданной маске;
Чтобы выбрать опцию, нужно ввести её номер и нажать Enter. 
Также в моей программе есть такая вещь как 'путь по умолчанию'. Это путь, в который ты попал через первые две функции.
Далее ты следуешь инструкциям от каждой конкретной опции.
После каждого выполненного действия тебе будет предложено либо продолжить, введя 'да',
либо закончить работу, введя что-то другое.
Удачи!
______________________________________________________________________________________________________________________";
            return instruction;
        }

        /// <summary>
        /// Основа программы.
        /// </summary>
        static void MiddleOfWork()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            uint optionNumber = EnterOfNumber("Введи номер опции(1-10), с которой хочешь работать: ",
                1, 10);
            OperationsDistribution(optionNumber);
        }

        /// <summary>
        /// Ввод числа пользователем.
        /// </summary>
        /// <param name="message">сообщение, выводимое для конкретизации числа.</param>
        /// <param name="lowBound">нижняя граница числа.</param>
        /// <param name="upBound">верхняя граница числа.</param>
        /// <returns>Введённое число.</returns>
        static uint EnterOfNumber(string message, uint lowBound, uint upBound)
        {
            uint number;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(message);
            while (!uint.TryParse(Console.ReadLine(), out number) || number < lowBound || number > upBound)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ты ввёл некорректный номер, попробуй ещё раз.");
                Console.Write(message);
            }
            return number;
        }

        /// <summary>
        /// Функция, вводящая путь до директории.
        /// </summary>
        /// <returns>Путь до директории.</returns>
        static string EnterOfPathToDirectory()
        {
            bool isUseDefault = DesireToUseDefaultPath();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Введи нужный путь: ");
            string path;
            // Вводим часть пути к директории пока она не будет существовать.
            while (!Directory.Exists(path = (isUseDefault ? 
                (defaultPath + ((defaultPath.Length == 0 || defaultPath[^1] == '\\') ?
                    "" : "\\")) : "") + Console.ReadLine()))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ты ввёл несуществующий путь, попробуй ещё раз.");
                Console.Write("Введи нужный путь: ");
            }
            return path;
        }
        
        /// <summary>
        /// Функция, вводящая путь до файла.
        /// </summary>
        /// <returns>Путь до файла</returns>
        static string EnterOfPathToFile()
        {
            bool isUseDefault = DesireToUseDefaultPath();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Введи нужный путь: ");
            string path;
            while (!File.Exists(path = (isUseDefault ? 
                (defaultPath + ((defaultPath.Length == 0 || defaultPath[^1] == '\\')
                    ? "" : "\\")) : "") + Console.ReadLine()))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ты ввёл несуществующий путь, попробуй ещё раз.");
                Console.Write("Введи нужный путь: ");
            }
            return path;
        }
        
        /// <summary>
        /// Функция, создающая путь до нового файла
        /// </summary>
        /// <param name="path">путь до директории, в которой будет находиться файл.</param>
        /// <param name="extension">разрешение файла.</param>
        /// <returns>Путь до нового файла.</returns>
        static string EnterOfNewFile(string path, string extension)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(@"Введи имя нового файла.  
(Только имя! Без разрешения.)
Файла с таким именем не должно существовать: ");
            string newPath;
            while (File.Exists(newPath = path + '\\' + Console.ReadLine() + extension))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ты ввёл имя существующего файла, попробуй ещё раз.");
                Console.Write("Введи новое имя: ");
            }
            return newPath;
        }
        
        /// <summary>
        /// Распределительное меню.
        /// </summary>
        /// <param name="optionNumber">номер операции, с которой хочет работать пользователь.</param>
        static void OperationsDistribution(uint optionNumber)
        {
            switch (optionNumber)
            {
                case 1:
                    defaultPath = ListOfDisks();
                    break;
                case 2:
                    defaultPath = ChoiceOfDirectory();
                    break;
                case 3:
                    ListOfFiles();
                    break;
                case 4:
                    OutputInEncoding();
                    break;
                case 5:
                    CopyingFile();
                    break;
                case 6:
                    MovingFiles();
                    break;
                case 7:
                    DeletingFile();
                    break;
                case 8:
                    CreatingFileInEncoding();
                    break;
                case 9:
                    FilesConcatenation();
                    break;
                default:
                    SearchingByMask();
                    break;
            }
        }

        /// <summary>
        /// Вывод и выбор из списка дисков.
        /// </summary>
        /// <returns>Выбранный диск.</returns>
        static string ListOfDisks()
        {
            try
            {
                uint i = 1;
                var listOfDisks = DriveInfo.GetDrives();
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Список дисков:");
                foreach (var el in listOfDisks)
                {
                    Console.WriteLine($"{i}. {el}");
                    i++;
                }
                uint numberOfDisk = EnterOfNumber($"Введи номер диска({1}-{i - 1}), с которым хочешь работать: ",
                    1, i - 1);
                string disk = listOfDisks[numberOfDisk - 1].ToString();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Хорошо! Теперь ты работаешь с диском {disk}");
                return disk;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Вывод и выбор из списка поддиректорий в директории.
        /// </summary>
        /// <returns>Выбранная директория.</returns>
        static string ChoiceOfDirectory()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($@"Тебе нужно ввести путь в место, в котором ты хочешь выбирать папку.
Если ты уже выбирал диск, то введи путь без диска. Пример: изначальный путь - C:\Path\Directory
Ты вводишь - Path\Directory
Если ты хочешь посмотреть директории по текущему пути, просто нажми Enter.
Текущий путь: {defaultPath}");
                uint i = 1;
                defaultPath = EnterOfPathToDirectory();
                var listOfDirectories = Directory.GetDirectories(defaultPath);
                if (listOfDirectories.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("В объекте, находящемся по данному пути, нет директорий.");
                    return defaultPath;
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Список директорий:");
                foreach (var el in listOfDirectories)
                {
                    Console.WriteLine($"{i}. {el}");
                    i++;
                }
                uint numberOfDirectory = EnterOfNumber($"Введи номер нужной папки({1}-{i - 1}): ",
                    1, i - 1);
                string directory = listOfDirectories[numberOfDirectory - 1];
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Хорошо! Теперь ты работаешь с папкой {directory}");
                return directory;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Вывод списка файлов в директории.
        /// </summary>
        static void ListOfFiles()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($@"Тебе нужно ввести путь в место, в котором ты хочешь смотреть список файлов.
Если ты уже выбирал папку или диск, то введи путь без них. Пример: изначальный путь - C:\Path\Directory
Ты вводишь - Path\Directory
Если ты хочешь посмотреть файлы по текущему пути, просто нажми Enter.
Текущий путь: {defaultPath}");
                uint i = 1;
                var listOfFiles = Directory.GetFiles(EnterOfPathToDirectory());
                if (listOfFiles.Length == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("В объекте, находящемся по данному пути, нет файлов.");
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Список файлов:");
                foreach (var el in listOfFiles)
                {
                    Console.WriteLine($"{i}. {el}");
                    i++;
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Желание использовать путь по умолчанию.
        /// </summary>
        /// <returns>Да/Нет</returns>
        static bool DesireToUseDefaultPath()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"Хочешь ли ты работать с путём по умолчанию? Выбери 1 или 2:
1. да
2. нет");
            switch (EnterOfNumber("Введи свой выбор(1-2): ", 1, 2))
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Вывод списка доступных кодировок.
        /// </summary>
        /// <returns>Список кодировок.</returns>
        static string Encodings()
        {
            return @"Напиши номер кодировки, с которой хочешь работать:
1. UTF-8
2. ASCII
3. Unicode";
        }
        
        /// <summary>
        /// Выбор кодировки.
        /// </summary>
        /// <returns>Выбранная кодировка.</returns>
        static Encoding ChoiceOfEncoding()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(Encodings());
            uint numberOfEncoding = EnterOfNumber("Введи номер кодировки(1-3): ", 1, 3);
            switch (numberOfEncoding)
            {
                case 1:
                    return Encoding.UTF8;
                case 2:
                    return Encoding.ASCII;
                default:
                    return Encoding.Unicode;
            }
        }
        
        /// <summary>
        /// Функция, выводящая содержимое файла в заданной пользователем кодировке.
        /// </summary>
        static void OutputInEncoding()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Тебе нужно ввести полный путь к файлу, содержимое которого хочешь вывести.");
                string path = EnterOfPathToFile();
                using (StreamReader sr = new StreamReader(path, ChoiceOfEncoding(), false))
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"В файле в заданной тобой кодировке написано:\n{sr.ReadToEnd()}");
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Функция, создающая копию файла.
        /// </summary>
        static void CopyingFile()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Тебе нужно ввести полный путь к файлу, содержимое которого хочешь вывести.");
                string path = EnterOfPathToFile();
                // Находим только имя нашего файла.
                string ourFile = path[(path.LastIndexOf('\\') + 1)..];
                string newFile = EnterOfNewFile(path[..(path.LastIndexOf('\\') + 1)], 
                    ourFile[ourFile.LastIndexOf('.')..]);
                File.Copy(path, newFile);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Твой файл '{ourFile}' был скопирован" +
                                  $" в файл '{newFile[(newFile.LastIndexOf('\\') + 1)..]}'");
                
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Перемещение файла в другую директорию.
        /// </summary>
        static void MovingFiles()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Тебе нужно ввести полный путь к файлу, который ты хочешь переместить.");
                string pathFile = EnterOfPathToFile();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Теперь надо ввести путь к директории, в которую мы перемещаем файл.");
                string pathDirectory = EnterOfPathToDirectory();
                File.Move(pathFile, pathDirectory + pathFile[pathFile.LastIndexOf('\\')..]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Твой файл '{pathFile[(pathFile.LastIndexOf('\\') + 1)..]}' был перемещен" +
                                  $" в директорию '{pathDirectory}'");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Удаление файла.
        /// </summary>
        static void DeletingFile()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Тебе нужно ввести полный путь к файлу, который ты хочешь удалить.");
                string pathFile = EnterOfPathToFile();
                File.Delete(pathFile);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Твой файл '{pathFile[(pathFile.LastIndexOf('\\') + 1)..]}' был удалён.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Создание файла в директории и запись в него текста в заданной кодировке.
        /// </summary>
        static void CreatingFileInEncoding()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Тебе нужно ввести полный путь к директории, в которой ты хочешь создать файл.");
                string pathFile = EnterOfNewFile(EnterOfPathToDirectory(), ".txt");
                File.Create(pathFile).Close();
                Console.Write("Введи текст, который хочешь записать в файл: ");
                File.WriteAllText(pathFile, Console.ReadLine(), ChoiceOfEncoding());
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Твой файл '{pathFile[(pathFile.LastIndexOf('\\') + 1)..]}' был создан.");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Конкатенация содержимого нескольких файлов.
        /// </summary>
        static void FilesConcatenation()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("В этой операции ты можешь выбрать количество файлов," +
                                  " которые хочешь конкатенировать - не менее 2 и не более 5.");
                uint quantity = EnterOfNumber("Введи количество файлов: ", 2, 5);
                string output = "";
                for (int i = 0; i < quantity; i++)
                {
                    string currentPath = EnterOfPathToFile();
                    using (StreamReader sr = new StreamReader(currentPath, Encoding.UTF8, false))
                    {
                        output += sr.ReadToEnd();
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Содержимое всех файлов:\n{output}");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Желание выводить файлы в поддиректориях.
        /// </summary>
        /// <returns>Да/Нет</returns>
        static bool DesireToSearchInAllDirectories()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"Хочешь ли ты вывести файлы поддиректорий? Выбери 1 или 2:
1. да
2. нет");
            switch (EnterOfNumber("Введи свой выбор(1-2): ", 1, 2))
            {
                case 1:
                    return true;
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Вывод файлов по маске.
        /// </summary>
        static void SearchingByMask()
        {
            try
            {
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@"Сейчас вам нужно ввести регулярное шарповское выражение,
которое будет описывать нужные вам файлы. Важно, что оно именно шарповское, его сразу будут подставлять в поиск.");
                Console.Write("Введи регулярное выражение: ");
                string expression = Console.ReadLine();
                string path = EnterOfPathToDirectory();
                string[] listOfFiles;
                if (DesireToSearchInAllDirectories())
                {
                    listOfFiles = Directory.GetFiles(path, expression, SearchOption.AllDirectories);
                }
                else
                {
                    listOfFiles = Directory.GetFiles(path, expression );
                }
                Console.ForegroundColor = ConsoleColor.White;
                if (listOfFiles.Length == 0)
                {
                    Console.WriteLine("Таких файлов нет.");
                }
                else
                {
                    Console.WriteLine("Список подходящих файлов:");
                    foreach (var file in listOfFiles)
                    {
                        Console.WriteLine(file);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
            }
        }
    }
}