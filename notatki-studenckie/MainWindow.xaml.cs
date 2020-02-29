using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace notatki_studenckie
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static System.Collections.ObjectModel.ObservableCollection<string> ImagePaths
        { get; set; }

        private static string pathXML = Directory.GetCurrentDirectory() + "/../../data/data.xml";
        private static string imageDirectory = Directory.GetCurrentDirectory() + "/../../data/img/";

        // zależne od języka
        private static string semesterText = "Semestr";
        private static string openImageDialogTitle = "Otwórz obrazy";

        private static System.Windows.Forms.OpenFileDialog openImageDialog;

        private static int selectedSemester = 0;
        private static string selectedSubject = "";
        private static string selectedNote = "";
        private static string selectedImagePath = "";


        private void SemesterAddButtonCheck()
        {
            if (addSemesterButton == null) return;
            string semesterNumber = SemesterNumberTextBox.Text.TrimStart('0');
            if (semesterNumber != null && semesterNumber != "" && Int32.Parse(semesterNumber) > 0)
            {
                string toAdd = semesterText + " " + semesterNumber;
                addSemesterButton.IsEnabled = !SemestersListBox.Items.Contains(toAdd);
            }
            else
            {
                addSemesterButton.IsEnabled = false;
            }
        }

        private void SubjectAddButtonCheck()
        {
            if (addSubjectButton == null) return;
            string subject = subjectTextBox.Text;
            if (subject != null && subject != "")
            {
                // sprawdzenie czy unikalny
                addSubjectButton.IsEnabled = !SubjectListBox.Items.Contains(subject);
            } else
            {
                addSubjectButton.IsEnabled = false;
            }
        }

        private void NoteAddButtonCheck()
        {
            if (addNoteButton == null)
            {
                return;
            }
            string note = NoteTextBox.Text;
            if (note != null && note != "")
            {
                // aktywowanie przycisku dodania tylko jeśli element jest unikalny
                addNoteButton.IsEnabled = !NoteListBox.Items.Contains(note);
            }
            else
            {
                addNoteButton.IsEnabled = false;
            }
        }

        private void EnableSubjectGUI(bool enable)
        {
            SubjectListBox.IsEnabled = enable;
            subjectTextBox.IsEnabled = enable;
            addSubjectButton.IsEnabled = enable;
        }

        private void EnableChooseNoteGUI(bool enable)
        {
            NoteListBox.Items.Clear();
            NoteListBox.IsEnabled = enable;
            NoteTextBox.IsEnabled = enable;
            addNoteButton.IsEnabled = enable;
        }

        private void EnableNoteGUI(bool enable)
        {
            if(!enable && selectedNote != "")
            {
                SaveNote();
            }

            MainNoteTextBox.Text = "";
            ImagePaths.Clear();

            MainNoteTextBox.IsEnabled = enable;
            ImagesListBox.IsEnabled = enable;
            addNewImageButton.IsEnabled = enable;
        }

        private void LoadSemestersFromXML()
        {
            XmlReader xmlReader = XmlReader.Create(pathXML);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "s"))
                {
                    AddSemester(xmlReader.GetAttribute("number"), false);
                }
            }
            xmlReader.Close();
        }

        private void LoadSubjectsFromXML()
        {
            SubjectListBox.Items.Clear();

            XmlReader xmlReader = XmlReader.Create(pathXML);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "subject"))
                {
                    if (xmlReader.GetAttribute("semester") == selectedSemester.ToString())
                    {
                        AddSubject(xmlReader.GetAttribute("name"), false);
                    }
                }
            }
            xmlReader.Close();
        }

        private void LoadNotesFromXML()
        {

            XmlReader xmlReader = XmlReader.Create(pathXML);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "note"))
                {
                    if (
                        Int32.Parse(xmlReader.GetAttribute("semester")) == selectedSemester
                        && xmlReader.GetAttribute("subject") == selectedSubject
                    )
                    {
                        AddNote(xmlReader.GetAttribute("name"), false);
                    }
                }

            }
            xmlReader.Close();
        }

        private void LoadTextNoteFromXML()
        {
            XmlReader xmlReader = XmlReader.Create(pathXML);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "note"))
                {
                    if (Int32.Parse(xmlReader.GetAttribute("semester")) == selectedSemester
                        && xmlReader.GetAttribute("subject") == selectedSubject
                        && xmlReader.GetAttribute("name") == selectedNote)
                    {
                        MainNoteTextBox.Text = xmlReader.GetAttribute("value");
                    }
                }

            }
            xmlReader.Close();
        }

        private void LoadImagesToListbox()
        {
            XmlReader xmlReader = XmlReader.Create(pathXML);
            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == "image"))
                {
                    if (Int32.Parse(xmlReader.GetAttribute("semester")) == selectedSemester
                        && xmlReader.GetAttribute("subject") == selectedSubject
                        && xmlReader.GetAttribute("noteName") == selectedNote)
                    {
                        AddImage(xmlReader.GetAttribute("fileName"), false);
                    }
                }

            }
            xmlReader.Close();
        }

        private void AddSemester(string semesterNumber, bool addToXML = false)
        {
            if (semesterNumber != null && semesterNumber != "" && Int32.Parse(semesterNumber) > 0)
            {
                string toAdd = semesterText + " " + semesterNumber;
                // sprawdzenie czy unikalny
                if (!SemestersListBox.Items.Contains(toAdd)) {

                    if (addToXML)
                    {
                        XDocument doc = XDocument.Load(pathXML);
                        doc.Root.Element("semesters").Add(
                            new XElement("s",
                                new XAttribute("number", semesterNumber)
                            )
                        );
                        try
                        {
                            doc.Save(pathXML);
                        }
                        catch
                        {
                           //MessageBox.Show("Błąd w zapisywaniu do pliku");
                           return;
                        }

                    }

                    SemestersListBox.Items.Add(toAdd);
                    SemesterAddButtonCheck();
                }

                // TODO sortowanie po nazwie
            }
        }
        
        private void AddSubject(string subject, bool addToXML = false)
        {
            if (subject != null && subject != "")
            {
                //item.Content = toAdd;
                // sprawdzenie czy unikalny
                if (!SubjectListBox.Items.Contains(subject))
                {

                    if (addToXML)
                    {
                        XDocument doc = XDocument.Load(pathXML);
                        doc.Root.Element("subjects").Add(
                            new XElement("subject",
                                new XAttribute("name", subject),
                                new XAttribute("semester", selectedSemester)
                            )
                        );
                        try
                        {
                            doc.Save(pathXML);
                        }
                        catch
                        {
                            //MessageBox.Show("Błąd w zapisywaniu do pliku");
                            return;
                        }

                    }

                    SubjectListBox.Items.Add(subject);
                    SubjectAddButtonCheck();
                }

                // TODO sortowanie po nazwie
            }
        }

        private void AddNote(string noteName, bool addToXML = false)
        {
            if (noteName != null && noteName != "")
            {
                //item.Content = toAdd;
                // sprawdzenie czy unikalny
                if (!NoteListBox.Items.Contains(noteName))
                {

                    if (addToXML)
                    {
                        XDocument doc = XDocument.Load(pathXML);
                        doc.Root.Element("notes").Add(
                            new XElement("note",
                                new XAttribute("semester", selectedSemester),
                                new XAttribute("name", noteName),
                                new XAttribute("subject", selectedSubject),
                                new XAttribute("value", "")
                            )
                        );
                        try
                        {
                            doc.Save(pathXML);
                        }
                        catch
                        {
                            //MessageBox.Show("Błąd w zapisywaniu do pliku");
                            return;
                        }

                    }

                    NoteListBox.Items.Add(noteName);
                    NoteAddButtonCheck();
                }

                // TODO sortowanie po nazwie
            }
        }

        private void AddImage(string imageFileName, bool addToXML = false)
        {
            if (imageFileName != null && imageFileName != "")
            {
                if (addToXML)
                {
                    XDocument doc = XDocument.Load(pathXML);
                    doc.Root.Element("images").Add(
                        new XElement("image",
                            new XAttribute("semester", selectedSemester),
                            new XAttribute("noteName", selectedNote),
                            new XAttribute("subject", selectedSubject),
                            new XAttribute("fileName", imageFileName)
                        )
                    );
                    try
                    {
                        doc.Save(pathXML);
                    }
                    catch
                    {
                        //MessageBox.Show("Błąd w zapisywaniu do pliku");
                        return;
                    }

                }

                ImagePaths.Add(imageDirectory + imageFileName);

                // TODO sortowanie (livesorting)
            }
        }

        private void PreviewImage()
        {
            var bitmapFrame = BitmapFrame.Create(new Uri(selectedImagePath), BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            double proportion = 1.0 * bitmapFrame.PixelWidth / bitmapFrame.PixelHeight;

            const int MAX_SIZE = 800;
            int windowWidth = MAX_SIZE;
            int windowHeight = (int)(1 / proportion * MAX_SIZE);
            if (proportion < 1)
            {
                windowWidth = (int)(proportion * MAX_SIZE);
                windowHeight = MAX_SIZE;
            }
            // korekta wymiarów na UI
            windowHeight += 40;

            Window1 imageViewerWindow = new Window1(selectedImagePath, windowWidth, windowHeight);
            imageViewerWindow.Show();
        }

        private void SaveNote()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathXML);

            XmlNode node = doc.SelectSingleNode("/data/notes/note[@name='" + selectedNote + "' and @subject='" + selectedSubject + "' and @semester='" + selectedSemester + "']");

            if (node != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    if (node.Attributes[i].Name == "value")
                    {
                        node.Attributes[i].Value = MainNoteTextBox.Text;
                    }
                }

                // weryfikowanie nowej struktury XML
                string newXML = doc.OuterXml;

                try
                {
                    doc.Save(pathXML);
                }
                catch
                {
                    //MessageBox.Show("Błąd w zapisywaniu do pliku");
                    return;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();

            ImagePaths = new System.Collections.ObjectModel.ObservableCollection<string>();

            LoadSemestersFromXML();

            EnableSubjectGUI(false);
            RemoveSubjectButton.IsEnabled = false;

            EnableChooseNoteGUI(false);
            RemoveNoteButton.IsEnabled = false;

            EnableNoteGUI(false);

            SemesterAddButtonCheck();

            openImageDialog = new System.Windows.Forms.OpenFileDialog();
            openImageDialog.Multiselect = true;
            openImageDialog.Title = openImageDialogTitle;
            openImageDialog.Filter = "Image Files (*.gif,*.jpg,*.jpeg,*.bmp,*.png)|*.gif;*.jpg;*.jpeg;*.bmp;*.png";

            ImagesListBox.ItemsSource = ImagePaths;
        }

        private void ListViewItem_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }
        
        private void OnlyNumbersTextBoxValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AddSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            string subject = subjectTextBox.Text;

            AddSubject(subject, true);
        }

        private void AddSemesterButton_Click(object sender, RoutedEventArgs e)
        {
            string semesterNumber = SemesterNumberTextBox.Text.TrimStart('0');

            AddSemester(semesterNumber, true);
        }

        private void SubjectListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // żeby aktywować funkcję zapisu
            EnableNoteGUI(false); 

            if (SubjectListBox.SelectedItem != null)
            {
                selectedSubject = SubjectListBox.SelectedItem.ToString();
                RemoveSubjectButton.IsEnabled = true;
                EnableChooseNoteGUI(true);
                LoadNotesFromXML();
            }
            else
            {
                selectedSubject = "";
                RemoveSubjectButton.IsEnabled = false;
                EnableChooseNoteGUI(false);
            }

        }

        private void NoteListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NoteListBox.SelectedItem != null)
            {
                // żeby aktywować funkcję zapisu
                EnableNoteGUI(false);

                selectedNote = NoteListBox.SelectedItem.ToString();
                RemoveNoteButton.IsEnabled = true;
                EnableNoteGUI(true);
                LoadTextNoteFromXML();
                LoadImagesToListbox();
            }
            else
            {
                selectedNote = "";
                RemoveNoteButton.IsEnabled = false;
                EnableNoteGUI(false);
            }
        }

        private void RemoveSubjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (NoteListBox.Items.Count == 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(pathXML);

                XmlNode node = doc.SelectSingleNode("/data/subjects/subject[@name='" + selectedSubject + "' and @semester='" + selectedSemester + "']");

                if (node != null)
                {
                    XmlNode parent = node.ParentNode;
                    parent.RemoveChild(node);
                    string newXML = doc.OuterXml;
                    try
                    {
                        doc.Save(pathXML);
                    }
                    catch
                    {
                        //MessageBox.Show("Błąd w zapisywaniu do pliku");
                        return;
                    }
                }

                int index = SubjectListBox.Items.IndexOf(SubjectListBox.SelectedItem);
                if (index != -1)
                {
                    SubjectListBox.Items.RemoveAt(index);
                    SubjectAddButtonCheck();
                }
            } else
            {
                MessageBox.Show("Najpierw usuń wszystkie notatki z tego przedmiotu");
            }
        }

        private void RemoveSemesterButton_Click(object sender, RoutedEventArgs e)
        {
            if (SubjectListBox.Items.Count == 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(pathXML);
                XmlNode node = doc.SelectSingleNode("/data/semesters/s[@number='" + selectedSemester + "']");
                if (node != null)
                {
                    XmlNode parent = node.ParentNode;
                    parent.RemoveChild(node);
                    string newXML = doc.OuterXml;
                    try
                    {
                        doc.Save(pathXML);
                    }
                    catch
                    {
                        //MessageBox.Show("Błąd w zapisywaniu do pliku");
                        return;
                    }
                }

                int index = SemestersListBox.Items.IndexOf(SemestersListBox.SelectedItem);
                if (index != -1)
                {
                    SemestersListBox.Items.RemoveAt(index);
                    SemesterAddButtonCheck();
                }
            } else
            {
                MessageBox.Show("Najpierw usuń wszystkie przedmioty z tego semestru");
            }
        }

        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            string subject = NoteTextBox.Text;

            AddNote(subject, true);
        }

        private void RemoveNoteButton_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathXML);

            XmlNode node = doc.SelectSingleNode("/data/notes/note[@name='" + selectedNote + "' and @subject='" + selectedSubject + "' and @semester='" + selectedSemester + "']");

            if (node != null)
            {
                XmlNode parent = node.ParentNode;
                parent.RemoveChild(node);
                string newXML = doc.OuterXml;
                try
                {
                    doc.Save(pathXML);
                }
                catch
                {
                    //MessageBox.Show("Błąd w zapisywaniu do pliku");
                    return;
                }
            }

            int index = NoteListBox.Items.IndexOf(NoteListBox.SelectedItem);
            if (index != -1)
            {
                NoteListBox.Items.RemoveAt(index);
                NoteAddButtonCheck();
            }
        }

        private void SemestersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableNoteGUI(false);
            EnableChooseNoteGUI(false);
            if (SemestersListBox.SelectedItem != null)
            {
                string semesterString = SemestersListBox.SelectedItem.ToString();
                string numberString = Regex.Match(semesterString, @"\d+").Value;
                int semesterNumber = Int32.Parse(numberString);
                selectedSemester = semesterNumber;
                RemoveSemesterButton.IsEnabled = true;
                EnableSubjectGUI(true);
                LoadSubjectsFromXML();
            } else
            {
                selectedSemester = 0;
                RemoveSemesterButton.IsEnabled = false;
                EnableSubjectGUI(false);
            }

        }



        private void SaveNoteButton_Click(object sender, RoutedEventArgs e)
        {
            SaveNote();
        }

        private void MainNoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void SubjectTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SubjectAddButtonCheck();
        }

        private void SemesterNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SemesterAddButtonCheck();
        }

        private void AddNewImageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = openImageDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                for (int i = 0; i < openImageDialog.FileNames.Count(); i++)
                {
                    string filePath = openImageDialog.FileNames[i];
                    string fileName = openImageDialog.SafeFileNames[i];
                    try
                    {
                        // sprawdzanie czy duplikat nazwy oraz kopiowanie do folderu lokalnego
                        string newFileName = fileName;
                        int number = 0;
                        while (File.Exists(imageDirectory + newFileName))
                        {
                            number++;
                            newFileName = System.IO.Path.GetFileName(fileName) + "_" + number + System.IO.Path.GetExtension(fileName);
                        }
                        File.Copy(filePath, imageDirectory + newFileName);

                        // dodawanie ścieżki obrazu do xml i do listboxa
                        AddImage(newFileName, true);
                    }
                    catch (System.Security.SecurityException ex)
                    {
                        Console.WriteLine(ex);
                        MessageBox.Show("Brak dostępu do pliku (brak uprawnień użytkownika do zapisu).");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        MessageBox.Show("Błąd wczytywania pliku (prawdopodobnie ma to związek z uprawnieniami systemu plików).");
                    }
                }
            }
        }

        private void ImagesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // mulitiselection na listboxach jest domyślnie wyłączone
            if(e.AddedItems.Count == 1)
            {
                selectedImagePath = e.AddedItems[0].ToString();
            }
            else if (e.RemovedItems.Count == 1)
            {
                selectedImagePath = "";
            }
        }

        private void DeleteImageContext_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dialogResult = MessageBox.Show("Czy na pewno usunąć zdjęcie?", "Usuwanie zdjęcia", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.No)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(pathXML);

            XmlNode node = doc.SelectSingleNode("/data/images/image[@noteName='" + selectedNote + "' and @subject='" + selectedSubject + "' and @semester='" + selectedSemester + "' and @fileName='" + System.IO.Path.GetFileName(selectedImagePath) + "']");

            if (node != null)
            {

                XmlNode parent = node.ParentNode;

                parent.RemoveChild(node);

                string newXML = doc.OuterXml;

                string imgPath = ImagesListBox.SelectedItem.ToString();
                try
                {
                    doc.Save(pathXML);

                    int index = ImagePaths.IndexOf(imgPath);
                    ImagePaths.RemoveAt(index);

                    // usuwanie pliku ze zdjeciem
                    File.Delete(selectedImagePath);
                }
                catch
                {
                    //addImage(imgPath, false);
                    //MessageBox.Show("Błąd w zapisywaniu do pliku");
                    return;
                }
            }
        }

        private void PreviewImageContext_Click(object sender, RoutedEventArgs e)
        {
            PreviewImage();
        }


        private void ImagesListBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void NoteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NoteAddButtonCheck();
        }

        private void MainWindow_Closing(Object sender, System.ComponentModel.CancelEventArgs e)
        {
            // selectedNote odnosi się tu do wcześniej zaznaczonej notatki
            if (selectedNote != "")
            {
                SaveNote();
            }
        }

        private void ImagesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PreviewImage();
        }
    }
}