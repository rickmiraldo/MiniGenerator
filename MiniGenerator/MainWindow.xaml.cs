using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Path = System.IO.Path;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TextBox = System.Windows.Controls.TextBox;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Input;
using MiniGenerator.Models;

namespace MiniGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> inputFiles = new List<string>();

        public MainWindow()
        {
            // Força o programa a usar ponto (.) como separador decimal no lugar de vírgula (,)
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            InitializeComponent();
        }

        private void btnChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            // Janela de escolher diretório
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }
                txtFolder.Text = dialog.SelectedPath;
                checkInputDirectory(dialog.SelectedPath);
            }
        }

        private bool checkInputDirectory(string path)
        {
            // Verificação checa se o diretório existe e chama outra função para contar e listar as imagens que existem
            if (Directory.Exists(path))
            {
                inputFiles.Clear();
                inputFiles = enumerateImagesInDirectory(path);
                txtStatusBar.Text = inputFiles.Count.ToString() + " arquivo(s) encontrado(s)";
                return true;
            }
            else
            {
                showWarning("Diretório inválido!");
                return false;
            }
        }

        private List<string> enumerateImagesInDirectory(string path)
        {
            // Atualmente somente verifica se existem arquivos TIF (inclui TIFF) e JPG
            // Não verifica subpastas
            List<string> tifFiles = Directory.EnumerateFiles(path, "*.tif", SearchOption.TopDirectoryOnly).ToList();
            List<string> jpgFiles = Directory.EnumerateFiles(path, "*.jpg", SearchOption.TopDirectoryOnly).ToList();

            List<string> files = new List<string>();

            files.AddRange(tifFiles);
            files.AddRange(jpgFiles);
            files.Sort();

            return files;
        }

        private void validateIfPositiveInteger(object sender, TextCompositionEventArgs e)
        {
            // Valida se o que foi digitado no campo de texto é um número inteiro positivo
            TextBox textBox = sender as TextBox;
            var newText = textBox.Text + e.Text;

            var regex = new Regex(@"^[0-9]*?$");

            if (regex.IsMatch(newText))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // Verificações de todos os campos antes de iniciar o processamento
            if (!verifyAllInputs())
            {
                return;
            }

            // Ler parâmetros de processamento
            var processingConfiguration = readProcessingConfiguration();
            if (processingConfiguration == null)
            {
                return;
            }

            blockInputs();

            int totalFiles = inputFiles.Count;
            pgrProgressBar.Value = 0;
            txtPercentageComplete.Text = "0%";

            string outputPath = Path.GetDirectoryName(inputFiles[0]) + "\\minis";

            var sw = new Stopwatch();
            sw.Restart();

            for (int i = 0; i < totalFiles; i++)
            {
                txtStatusBar.Text = "Gerando " + (i + 1) + " de " + totalFiles + " minis...";
                await Task.Run(() => GenerateMini.StartProcessing(inputFiles[i], outputPath, processingConfiguration));

                var percentageComplete = (100 * (i + 1)) / totalFiles;
                pgrProgressBar.Value = percentageComplete;
                txtPercentageComplete.Text = percentageComplete.ToString() + "%";
            }

            sw.Stop();

            string totalTime = convertTime(sw.ElapsedMilliseconds);
            float average = (sw.ElapsedMilliseconds / 1000f) / totalFiles;
            txtStatusBar.Text = "Geradas " + totalFiles + " minis em " + totalTime + ". Média: " + average.ToString("0.0") + "s/mini.";

            unblockInputs();
        }

        private bool verifyAllInputs()
        {
            // Verificar diretório de entrada
            if (!checkInputDirectory(txtFolder.Text))
            {
                return false;
            }

            // Verificar se há arquivos para processar
            if (!inputFiles.Any())
            {
                showWarning("Não há arquivos para processar na pasta selecionada!");
                return false;
            }

            // Verificar fator de escala
            if (txtFactor.Text == "")
            {
                showWarning("Fator de escala inválido!");
                return false;
            }

            return true;
        }

        private ProcessingConfiguration readProcessingConfiguration()
        {
            int factor = txtFactor.Text == "" ? 0 : int.Parse(txtFactor.Text);
            int border = txtThickness.Text == "" ? 0 : int.Parse(txtThickness.Text);

            var config = new ProcessingConfiguration(factor, border);

            return config;
        }

        private string convertTime(float ellapsedTime)
        {
            float seconds = ellapsedTime / 1000f;

            if (seconds < 60)
            {
                return seconds.ToString("0.00") + "s";
            }
            else
            {
                int minutes = (int)(seconds / 60);
                seconds = seconds - (minutes * 60);
                return minutes + "m" + seconds.ToString("0") + "s";
            }
        }

        private void blockInputs()
        {
            txtFolder.IsEnabled = false;
            btnChooseFolder.IsEnabled = false;
            txtFactor.IsEnabled = false;
            txtThickness.IsEnabled = false;
            btnStart.IsEnabled = false;
        }

        private void unblockInputs()
        {
            txtFolder.IsEnabled = true;
            btnChooseFolder.IsEnabled = true;
            txtFactor.IsEnabled = true;
            txtThickness.IsEnabled = true;
            btnStart.IsEnabled = true;
        }

        private void btnSobre_Click(object sender, RoutedEventArgs e)
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mini Generator v" + version);
            sb.AppendLine();
            sb.AppendLine("BASE Aerofotogrametria e Projetos S.A.");
            sb.AppendLine("Henrique G. Miraldo");

            MessageBox.Show(sb.ToString(), "Sobre", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSair_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void showWarning(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
