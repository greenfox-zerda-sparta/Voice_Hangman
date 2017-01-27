using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;

namespace HangmanFedEx {
  public partial class MainWindow:Window {

    // VARIABLES:

    private SpeechSynthesizer ss = new SpeechSynthesizer();
    private SpeechRecognitionEngine sre;
    private List<Label> Labels = new List<Label>();
    private List<Shape> HangedMan = new List<Shape>();

    private int Tries = 0;
    private string ShownWord;
    private string HiddenWord;
    private string RecognizedSpeech;
    private bool IsGameOver = false;

    // MAIN:

    public MainWindow() {
      InitializeComponent();
      InitializeElements();
      ss.SpeakAsync("Welcome to Voice Controlled Hangman!");
    }

    // INITIALIZE NEW GAME:

    public void InitializeElements() {
      InitializeLabels();
      InitializeHangedMan();
      InitializeVoiceRecognition();
      GenerateHiddenWord();
      GenerateShownWord();
      PrintShownWord();
    }

    public void ResetVariables() {
      ShownWord = "";
      HiddenWord = "";
      RecognizedSpeech = "";
      Tries = 0;
      IsGameOver = false;
      YouWon.Foreground = Brushes.Transparent;
      YouLost.Foreground = Brushes.Transparent;

      foreach(FrameworkElement item in (UI.Children)) {
        if(item is Button) {
          item.IsEnabled = true;
        }
      }

      foreach(ContentControl item in (UI.Children)) {
        if(item is Label) {
          item.Content = "";
        }
      }

      foreach(Shape item in (HangMan.Children)) {
        item.Stroke = Brushes.Transparent;
      }

    }

    // GAME LOGIC:

    public void GenerateHiddenWord() {
      List<string> WordsFromFile = System.IO.File.ReadAllLines(@"words.txt").ToList();
      WordsFromFile.RemoveAll(x => x.Length < 4);
      WordsFromFile.RemoveAll(x => x.Length > 10);
      Random Rnd = new Random();
      HiddenWord = WordsFromFile[Rnd.Next(WordsFromFile.Count)];
    }

    public void GenerateShownWord() {
      for(int i = 0; i < HiddenWord.Length; i++) {
        ShownWord += "_";
      }
    }

    public void PrintShownWord() {
      for(int i = 0; i < HiddenWord.Length; i++) {
        Labels[i].Content = ShownWord[i];
      }
    }

    public void CheckGuess(string letter) {
      int MatchCounter = 0;
      for(int i = 0; i < HiddenWord.Length; i++) {
        if(HiddenWord[i] == letter[0]) {
          ShownWord = ShownWord.Remove(i, 1).Insert(i, letter);
          MatchCounter++;
        }
      }
      if(MatchCounter == 0) {
        HangedMan[Tries].Stroke = Brushes.White;
        Tries++;
      }
    }

    public void GameOver() {
      if(ShownWord == HiddenWord && Tries < 10) {
        ss.SpeakAsync("Congratulations, you won!");
        YouWon.Foreground = Brushes.LawnGreen;
        IsGameOver = true;
      }
      if(Tries == 10) {
        ShownWord = HiddenWord;
        PrintShownWord();
        ss.SpeakAsync("Game over! You lost!");
        YouLost.Foreground = Brushes.Crimson;
        IsGameOver = true;
      }
    }

    // EVENT HANDLING:

    private void ClickOnLetter(object sender, RoutedEventArgs e) {
      if(!IsGameOver) {
        Button LetterButton = (Button)sender;
        LetterButton.IsEnabled = false;
        string ClickedLetter = LetterButton.Name.ToString();
        CheckGuess(ClickedLetter);
        PrintShownWord();
        GameOver();
      }
    }

    private void ClickOnNewGame(object sender, RoutedEventArgs e) {
      ResetVariables();
      InitializeElements();
    }

    // VOICE RECOGNITION:

    private void InitializeVoiceRecognition() {
      CultureInfo CI = new CultureInfo("en-us");
      sre = new SpeechRecognitionEngine(CI);
      sre.SetInputToDefaultAudioDevice();
      ss.SetOutputToDefaultAudioDevice();
      sre.SpeechRecognized += SpeechRecognition;

      Choices CHGuesses = new Choices();

      CHGuesses.Add("Is the word include");
      CHGuesses.Add("Does it have");
      CHGuesses.Add("What about");

      CHGuesses.Add("letter A");
      CHGuesses.Add("letter B");
      CHGuesses.Add("letter C");
      CHGuesses.Add("letter D");
      CHGuesses.Add("letter E");
      CHGuesses.Add("letter F");
      CHGuesses.Add("letter G");
      CHGuesses.Add("letter H");
      CHGuesses.Add("letter I");
      CHGuesses.Add("letter J");
      CHGuesses.Add("letter K");
      CHGuesses.Add("letter L");
      CHGuesses.Add("letter M");
      CHGuesses.Add("letter N");
      CHGuesses.Add("letter O");
      CHGuesses.Add("letter P");
      CHGuesses.Add("letter Q");
      CHGuesses.Add("letter R");
      CHGuesses.Add("letter S");
      CHGuesses.Add("letter T");
      CHGuesses.Add("letter U");
      CHGuesses.Add("letter V");
      CHGuesses.Add("letter W");
      CHGuesses.Add("letter X");
      CHGuesses.Add("letter Y");
      CHGuesses.Add("letter Z");

      GrammarBuilder GBGuesses = new GrammarBuilder();
      GBGuesses.Append(CHGuesses);
      Grammar GGuesses = new Grammar(GBGuesses);
      sre.LoadGrammarAsync(GGuesses);

      Choices CHCommands = new Choices();
      CHCommands.Add("Start game");
      CHCommands.Add("New game");
      CHCommands.Add("Restart");
      CHCommands.Add("Start over");
      CHCommands.Add("Give up");
      CHCommands.Add("exit");

      GrammarBuilder GBCommands = new GrammarBuilder();
      GBCommands.Append(CHCommands);
      Grammar GCommands = new Grammar(GBCommands);
      sre.LoadGrammarAsync(GCommands);

      sre.RecognizeAsync(RecognizeMode.Multiple);
    }

    public void IsLetterRecognized(Button letter, string RecognizedText, string RecognizedLetter) {
      if(RecognizedText.IndexOf(RecognizedLetter) >= 0) {
        if(letter.IsEnabled == true) {
          letter.IsEnabled = false;
          string ClickedLetter = letter.Name.ToString();
          CheckGuess(ClickedLetter);
          PrintShownWord();
          GameOver();
        }
      }
    }

    public void SpeechRecognition(object sender, SpeechRecognizedEventArgs ev) {
      RecognizedSpeech = ev.Result.Text;
      float Confidence = ev.Result.Confidence;

      if(Confidence < 0.60)
        return;

      if(RecognizedSpeech.IndexOf("New game") >= 0 || RecognizedSpeech.IndexOf("Restart") >= 0 || RecognizedSpeech.IndexOf("Start over") >= 0) {
        ResetVariables();
        InitializeElements();
      }

      if(RecognizedSpeech.IndexOf("Give up") >= 0) {
        Tries = 10;
        GameOver();
      }

      if(!IsGameOver) {
        IsLetterRecognized(a, RecognizedSpeech, "letter A");
        IsLetterRecognized(b, RecognizedSpeech, "letter B");
        IsLetterRecognized(c, RecognizedSpeech, "letter C");
        IsLetterRecognized(d, RecognizedSpeech, "letter D");
        IsLetterRecognized(e, RecognizedSpeech, "letter E");
        IsLetterRecognized(f, RecognizedSpeech, "letter F");
        IsLetterRecognized(g, RecognizedSpeech, "letter G");
        IsLetterRecognized(h, RecognizedSpeech, "letter H");
        IsLetterRecognized(i, RecognizedSpeech, "letter I");
        IsLetterRecognized(j, RecognizedSpeech, "letter J");
        IsLetterRecognized(k, RecognizedSpeech, "letter K");
        IsLetterRecognized(l, RecognizedSpeech, "letter L");
        IsLetterRecognized(m, RecognizedSpeech, "letter M");
        IsLetterRecognized(n, RecognizedSpeech, "letter N");
        IsLetterRecognized(o, RecognizedSpeech, "letter O");
        IsLetterRecognized(p, RecognizedSpeech, "letter P");
        IsLetterRecognized(q, RecognizedSpeech, "letter Q");
        IsLetterRecognized(r, RecognizedSpeech, "letter R");
        IsLetterRecognized(s, RecognizedSpeech, "letter S");
        IsLetterRecognized(t, RecognizedSpeech, "letter T");
        IsLetterRecognized(u, RecognizedSpeech, "letter U");
        IsLetterRecognized(v, RecognizedSpeech, "letter V");
        IsLetterRecognized(w, RecognizedSpeech, "letter W");
        IsLetterRecognized(x, RecognizedSpeech, "letter X");
        IsLetterRecognized(y, RecognizedSpeech, "letter Y");
        IsLetterRecognized(z, RecognizedSpeech, "letter Z");
      }

      if(RecognizedSpeech.IndexOf("exit") >= 0) {
        ((SpeechRecognitionEngine)sender).RecognizeAsyncCancel();
        ss.Speak("Farewell");
        Environment.Exit(0);
      }
    }

    // UTILS

    private void InitializeLabels() {
      Labels.Add(label1);
      Labels.Add(label2);
      Labels.Add(label3);
      Labels.Add(label4);
      Labels.Add(label5);
      Labels.Add(label6);
      Labels.Add(label7);
      Labels.Add(label8);
      Labels.Add(label9);
      Labels.Add(label10);
    }

    private void InitializeHangedMan() {
      HangedMan.Add(fault1);
      HangedMan.Add(fault2);
      HangedMan.Add(fault3);
      HangedMan.Add(fault4);
      HangedMan.Add(fault5);
      HangedMan.Add(fault6);
      HangedMan.Add(fault7);
      HangedMan.Add(fault8);
      HangedMan.Add(fault9);
      HangedMan.Add(fault10);
    }

    private void MinimizeWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      WindowState = WindowState.Minimized;
    }

    private void CloseWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      Close();
    }

    private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      DragMove();
    }
  }
}