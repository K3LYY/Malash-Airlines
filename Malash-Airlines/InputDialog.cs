using System;
using System.Windows;
using System.Windows.Controls;

namespace Malash_Airlines {
    public class InputDialog : Window {
        private TextBox responseTextBox;

        public string ResponseText {
            get { return responseTextBox.Text; }
            set { responseTextBox.Text = value; }
        }

        public InputDialog(string title, string promptText, string defaultValue = "") {
            this.Title = title;
            this.Width = 350;
            this.Height = 170;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ResizeMode = ResizeMode.NoResize;

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            TextBlock promptLabel = new TextBlock();
            promptLabel.Text = promptText;
            promptLabel.Margin = new Thickness(0, 0, 0, 5);
            Grid.SetRow(promptLabel, 0);
            grid.Children.Add(promptLabel);

            responseTextBox = new TextBox();
            responseTextBox.Text = defaultValue;
            responseTextBox.Margin = new Thickness(0, 0, 0, 15);
            responseTextBox.Padding = new Thickness(5);
            responseTextBox.Height = 30;
            Grid.SetRow(responseTextBox, 1);
            grid.Children.Add(responseTextBox);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetRow(buttonPanel, 2);
            grid.Children.Add(buttonPanel);

            Button okButton = new Button();
            okButton.Content = "OK";
            okButton.Padding = new Thickness(15, 5, 15, 5);
            okButton.Margin = new Thickness(0, 0, 10, 0);
            okButton.Click += OkButton_Click;
            okButton.IsDefault = true;
            buttonPanel.Children.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.Content = "Anuluj";
            cancelButton.Padding = new Thickness(15, 5, 15, 5);
            cancelButton.Click += CancelButton_Click;
            cancelButton.IsCancel = true;
            buttonPanel.Children.Add(cancelButton);

            this.Content = grid;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
