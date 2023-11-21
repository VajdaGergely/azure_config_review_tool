using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace azure_administration_tool1
{
    public class DataGridViewCustom : DataGridView
    {
        //CheckClipboardMethods
        public void ClipboardKeys(KeyEventArgs e, int[] enabledToPasteColIndexes)
        {
            CtrlC(e);
            CtrlV(e, enabledToPasteColIndexes);
            CtrlX(e, enabledToPasteColIndexes);
        }

        public void CtrlC(KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.C && e.Control && this.SelectedCells.Count == 1)
                {
                    if (this.SelectedCells[0].Value != null && this.SelectedCells[0].Value.ToString() != "")
                    {
                        Clipboard.SetText(this.SelectedCells[0].Value.ToString(), TextDataFormat.Text);
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                }
            }
            catch
            {
                MessageBox.Show("ClipBoard error! No problem. Try it again slowly!", "Clipboard error");
            }
        }

        public void CtrlV(KeyEventArgs e, int[] enabledToPasteColIndexes)
        {
            try
            {
                if (e.KeyCode == Keys.V && e.Control)
                {
                    if (Clipboard.GetText(TextDataFormat.Text) != string.Empty && Clipboard.GetText(TextDataFormat.Text) != null &&
                        Clipboard.GetText(TextDataFormat.Text) != "")
                    {
                        foreach (DataGridViewCell cell in this.SelectedCells)
                        {
                            if (enabledToPasteColIndexes.Contains(cell.ColumnIndex))
                            {
                                cell.Value = Clipboard.GetText(TextDataFormat.Text);
                            }
                        }
                    }
                    else
                    {
                        foreach (DataGridViewCell cell in this.SelectedCells)
                        {
                            if (enabledToPasteColIndexes.Contains(cell.ColumnIndex))
                            {
                                cell.Value = "";
                            }
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("ClipBoard error! No problem. Try it again slowly!", "Clipboard error");
            }
        }

        public void CtrlX(KeyEventArgs e, int[] enabledToPasteColIndexes)
        {
            try
            {
                if (e.KeyCode == Keys.X && e.Control && this.SelectedCells.Count == 1 &&
                    enabledToPasteColIndexes.Contains(this.SelectedCells[0].ColumnIndex))
                {
                    if (this.SelectedCells[0].Value != null && this.SelectedCells[0].Value.ToString() != "")
                    {
                        Clipboard.SetText(this.SelectedCells[0].Value.ToString(), TextDataFormat.Text);
                        this.SelectedCells[0].Value = "";
                    }
                    else
                    {
                        Clipboard.Clear();
                    }
                }
            }
            catch
            {
                MessageBox.Show("ClipBoard error! No problem. Try it again slowly!", "Clipboard error");
            }
        }

        public void Del(KeyEventArgs e, int[] enabledToPasteColIndexes)
        {
            try
            {
                if (e.KeyCode == Keys.Delete)
                {
                    foreach (DataGridViewCell cell in this.SelectedCells)
                    {
                        if (enabledToPasteColIndexes.Contains(cell.ColumnIndex))
                        {
                            cell.Value = "";
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("ClipBoard error! No problem. Try it again slowly!", "Clipboard error");
            }
        }
    }
}
