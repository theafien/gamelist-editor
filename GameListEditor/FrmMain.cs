using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GameListEditor
{
    public partial class FrmMain : Form
    {

        public class BackgroundWorkerItem
        {
            public int index;
            public GameObject gameObject;
        }
        string mFilePath;
        string mTitle;

        GameListObject mGameListObject;

        public FrmMain()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            LoadXMLFile();
            RefreshListView();
        }

        private void RefreshListView()
        {
            txtOriginalText.Text = "";
            txtProcessText.Text = "...";
            txtStatus.Text = "Ready";
            txtTranslatedText.Text = "";

            lvGameList.Items.Clear();

            if (mGameListObject == null || mGameListObject.gameList.Count == 0)
            {
                return;
            }

            foreach (GameObject gameObject in mGameListObject.gameList)
            {
                ListViewItem item = new ListViewItem(gameObject.translated ? "Yes" : "No");

                item.SubItems.Add(gameObject.name);
                item.SubItems.Add(gameObject.developer);
                item.SubItems.Add(gameObject.publisher);
                item.SubItems.Add(gameObject.genre);

                if (gameObject.translated)
                {
                    item.BackColor = Color.Aqua;
                }
                else
                {
                    item.BackColor = Color.Empty;
                }


                item.Tag = gameObject;

                lvGameList.Items.Add(item);


            }

            txtStatus.Text = String.Format("{0} items", mGameListObject.gameList.Count);
        }
        private bool LoadXMLFile()
        {
            if (OpenFile())
            {
                mGameListObject = GameListObject.LoadFromFile(mFilePath);
                return mGameListObject != null;
            }
            return false;
        }
        private bool OpenFile()
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mFilePath = openFileDialog.FileName;
                return true;
            }
            return false;
        }

        private void SetTitle(string text)
        {
            Text = String.Format("{0} - {1}", mTitle, text);
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            mTitle = Text;
        }

        private void selectedListViewItem(GameObject gameObject)
        {
            if (gameObject.translated)
            {
                txtOriginalText.Text = formatMultilines(gameObject.desc_original);
                txtTranslatedText.Text = formatMultilines(gameObject.desc);
            }
            else
            {
                if (String.IsNullOrEmpty(gameObject.desc))
                {
                    txtOriginalText.Text = "";
                    txtTranslatedText.Text = "";
                }
                else
                {
                    txtOriginalText.Text = formatMultilines(gameObject.desc);
                    txtTranslatedText.Text = "";
                }

            }
        }

        private void lvGameList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                selectedListViewItem((GameObject)e.Item.Tag);
            }
        }

        private string formatMultilines(string text)
        {
            List<string> lines = new List<string>(text.Split(new char[] { '\n' }));
            return String.Join(Environment.NewLine, lines.Select(t => t.Trim()));
        }

        private void btnTranslateText_Click(object sender, EventArgs e)
        {
            GameObject gameObject = (GameObject)lvGameList.SelectedItems[0].Tag;

            if (TranslateTextFromGameObject(gameObject))
            {
                UpdateListViewItem(lvGameList.SelectedItems[0], gameObject);
                UpdateTextBoxes(gameObject);
            }
        }

        private void UpdateTextBoxes(GameObject gameObject)
        {
            if (gameObject.translated)
            {
                txtTranslatedText.Text = gameObject.desc;
            }
            else
            {
                txtTranslatedText.Text = "";
            }

        }

        private void UpdateListViewItem(ListViewItem listViewItem, GameObject gameObject)
        {
            if (gameObject.translated)
            {
                listViewItem.Text = "Yes";
                listViewItem.BackColor = Color.Aqua;
            }
            else
            {
                listViewItem.Text = "No";
                listViewItem.BackColor = Color.Empty;
            }

        }



        private bool TranslateTextFromGameObject(GameObject gameObject)
        {
            if (!gameObject.translated && !String.IsNullOrEmpty(gameObject.desc))
            {
                string translatedText = GoogleTranslate.Translate(gameObject.desc, "en", "pt");
                if (!String.IsNullOrEmpty(translatedText))
                {
                    gameObject.desc_original = gameObject.desc;
                    gameObject.desc = translatedText;
                    gameObject.translated = true;
                    return true;
                }
            }
            return false;
        }


        BackgroundWorker mBackgroundWorker;
        ListView.ListViewItemCollection mListViewCollection;

        private void btnTranslateAll_Click(object sender, EventArgs e)
        {
            if (mBackgroundWorker != null || mGameListObject.gameList.Count == 0)
            {
                return;
            }

            pbTranslatedProcess.Maximum = mGameListObject.gameList.Count;

            mBackgroundWorker = new BackgroundWorker();
            mBackgroundWorker.DoWork += mBackgroundWorker_DoWork;
            mBackgroundWorker.RunWorkerCompleted += mBackgroundWorker_RunWorkerCompleted;
            mBackgroundWorker.ProgressChanged += mBackgroundWorker_ProgressChanged;
            mBackgroundWorker.WorkerReportsProgress = true;
            mBackgroundWorker.WorkerSupportsCancellation = true;
            mBackgroundWorker.RunWorkerAsync();

            btnCancel.Enabled = true;
            btnTranslateAll.Enabled = false;

            txtStatus.Text = "Translating";
        }

        private void mBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorkerItem item = (BackgroundWorkerItem)e.UserState;
            ListViewItem listViewItem = lvGameList.Items[item.index];
            UpdateListViewItem(listViewItem, item.gameObject);
            UpdateTranslateProcess(e.ProgressPercentage);
        }

        private void UpdateTranslateProcess(int p)
        {
            pbTranslatedProcess.Value = p;
        }

        private void mBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            mBackgroundWorker = null;
            btnCancel.Enabled = false;
            btnTranslateAll.Enabled = true;
            txtStatus.Text = "Ready";
        }

        private void mBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int nItem = 0;
            foreach (GameObject item in mGameListObject.gameList)
            {
                if (mBackgroundWorker == null || mBackgroundWorker.CancellationPending)
                {
                    break;
                }
                if (TranslateTextFromGameObject(item))
                {
                    mBackgroundWorker.ReportProgress(nItem, new BackgroundWorkerItem()
                    {
                        index = nItem,
                        gameObject = item
                    });
                }
                nItem++;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (mBackgroundWorker != null)
            {
                mBackgroundWorker.CancelAsync();
            }
            btnCancel.Enabled = false;
            btnTranslateAll.Enabled = true;
        }

        private void btnSaveFile_ButtonClick(object sender, EventArgs e)
        {
            SaveFile(mFilePath);
        }

        private void SaveFile(string filename)
        {
            if (mGameListObject.SaveFile(filename))
            {
                MessageBox.Show(this, "File saved successfully!");
            }
            else
            {
                MessageBox.Show(this, "Error saving file!");
            }
        }

        private void btnSaveFileAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void btnSaveData_Click(object sender, EventArgs e)
        {
            GameObject gameObject = (GameObject)lvGameList.SelectedItems[0].Tag;

            if (!String.IsNullOrEmpty(txtTranslatedText.Text))
            {
                gameObject.translated = true;
                gameObject.desc = txtTranslatedText.Text;
                gameObject.desc_original = txtOriginalText.Text;
            }
            else
            {
                gameObject.translated = false;
                gameObject.desc = txtOriginalText.Text;
                gameObject.desc_original = "";
            }

            UpdateListViewItem(lvGameList.SelectedItems[0], gameObject);
        }

    }
}
