using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace IAmyMyOwnOrg.Sudoku
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class SudokuControl : UserControl
    {
        #region vars
        const int GRID_SIZE = 9;
        const int BOX_SIZE = 3;
        const int BLANK = 0;
        private static SolidColorBrush[] m_Colors;
        long checkCount = 0;
        private static SudokuControl Main;
        private static Rectangle _rect;
        private Thread _thread1;
        #endregion

        //NOTE: Everytime I try to modify this the app hangs!
        
        public SudokuControl()
        {            
            InitializeComponent();
            Main = this; 
            _rect = slot0;//PuzzleGrid.Children.ElementAt(0) as Rectangle;
            this.NewPuzzleBtn_Click(null, null);
        }

        [PreEmptive.Attributes.Setup(CustomEndpoint = "so-s.info/PreEmptive.Web.Services.Messaging/MessagingServiceV2.asmx")]
        [PreEmptive.Attributes.Teardown()]
        private void InitializeColors() {
            m_Colors = new SolidColorBrush[GRID_SIZE+1];

            // This array will be used to color the grid based on value
            m_Colors[0] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // The blank
            m_Colors[1] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
            m_Colors[2] = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            m_Colors[3] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            m_Colors[4] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            m_Colors[5] = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
            m_Colors[6] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255));

            m_Colors[7] = new SolidColorBrush(Color.FromArgb(255, 125, 125, 125));
            m_Colors[8] = new SolidColorBrush(Color.FromArgb(255, 125, 0, 255));
            m_Colors[9] = new SolidColorBrush(Color.FromArgb(255, 0, 125, 255));
        }

        private void SetSlotValue(int i, int j, int value) {
            //IEnumerator asfd = PuzzleGrid.Children.GetEnumerator();            
            
            (PuzzleGrid.Children.Cast<Rectangle>().ElementAt(i + j * GRID_SIZE) as Rectangle).Fill = m_Colors[value];

            //Redraw the screen for the user to see what's up.
            //App.Current.RootVisual.Visibility = Visibility.Collapsed;
            //App.Current.RootVisual.Visibility = Visibility.Visible;
            this.Visibility = Visibility.Visible;
            
        }

        private void SetSlotValue(int i, int value) {
            // Check for a valid value; otherwise, set it to zero
            if (value >= m_Colors.Length || value < 0)
                value = 0;

            // Set the slot value by selecting the color that matches
            (PuzzleGrid.Children.Cast<Rectangle>().ElementAt(i) as Rectangle).Fill = m_Colors[value];
        }

        private int GetSlotValue(Rectangle slot) {
            for (int i = 0; i < 10; i++) {
                if (slot.Fill == m_Colors[i])
                    return i;
            }

            return 0; //blank is the default
        }

        private void LoadPuzzle(string in_grid) 
        {                        
            for (int i = 0; i < 81; i++) {
                Rectangle slot = PuzzleGrid.Children.Cast<Rectangle>().ElementAt(i) as Rectangle;
                if (in_grid[i] == '.') {
                    SetSlotValue(i, BLANK);
                } else {
                    // Convert the ASCII values to integer equivalents.
                    SetSlotValue(i, (in_grid[i]-48));                    
                }
            }                        
        }

        private void createNewPuzzle(int diff)
        {
	        int r, c, i, n;
	        int[] seed = { 0, 3, 6, 1, 4, 7, 2, 5, 8 };

            for (c = 0; c < 9; c++) {
                for (r = 0; r < 9; r++) {
                    //Rectangle slot = PuzzleGrid.Children.ElementAt(c * r + c) as Rectangle;
                    SetSlotValue((c * r + c), ((seed[c] + r) % 9) + 1);                    
                }            
            }
            
            Random rand = new Random();
	        n = 1<<(2+diff);
            n = rand.Next(0, 80);
            for (i = 0; i < n; i++) {                
                //Rectangle slot = PuzzleGrid.Children.ElementAt(rand.Next(0,80) + 1) as Rectangle;
                SetSlotValue((rand.Next(0, 80) + 1), BLANK);
            }		        
        }

        /*
         * Rountine to validate puzzle
         *
         * AJN - Changed isSolved and isValid to return 1 if solution,
         *       -1 if legal but not solved and 0 if illegal.
         *
         */
        private int isValid( ref int[] v )
        {
	        int i, s;
            int[] a = new int[GRID_SIZE];

	        for( i=0; i<GRID_SIZE; i++ )
		        a[i] = 0;

	        for( i=0; i<GRID_SIZE; i++ )
		        if ( v[i] > 0 )
			        a[v[i]-1]++;

	        for( i=0, s=0; i<GRID_SIZE; i++ )
		        if ( a[i] > 1 )
			        return -1;
		        else if ( a[i] > 0 )
			        s += a[i];

	        return( s == 9 ? 1 : -1 );
            
        }

        private int isSolved()
        {
	        int i, j, k, l;
            int[] v = new int[GRID_SIZE];
	        int c1, c2, c3;
            c1 = c2 = c3 = 0;
	        checkCount++;
	        //if ( !(checkCount % 1000000) )
		    //    printf( "%ld, ", checkCount );

	        for( i=0; i<GRID_SIZE; i++ ) // check rows
	        {
                for (j = 0; j < GRID_SIZE; j++)
                    v[j] = GetSlotValue(PuzzleGrid.Children.Cast<Rectangle>().ElementAt(j + i * GRID_SIZE) as Rectangle);
			        
		        //if ( !(c1 = isValid( ref v )) )
                c1 = isValid(ref v);
                if( c1 == -1 )
			        return( -1 );
	        }

	        for( j=0; j<GRID_SIZE; j++ ) // check columns
	        {
		        for( i=0; i<GRID_SIZE; i++ )
                    v[i] = GetSlotValue(PuzzleGrid.Children.Cast<Rectangle>().ElementAt(j + i * GRID_SIZE) as Rectangle);
		        //if ( !(c2 = isValid( v )) )
                c2 = isValid(ref v);
                if(c2 == -1)
			        return( -1 );
	        }

	        for( i=0; i<BOX_SIZE; i++ ) // check boxes
		        for( j=0; j<BOX_SIZE; j++ )
		        {
			        for( k=0; k<BOX_SIZE; k++ )
				        for( l=0; l<BOX_SIZE; l++ )
                            v[k * BOX_SIZE + l] = GetSlotValue(PuzzleGrid.Children.Cast<Rectangle>().ElementAt((j * BOX_SIZE + l) + (i * BOX_SIZE + k) * GRID_SIZE) as Rectangle);
					        //v[k*BOX_SIZE+l] = grid[i*BOX_SIZE+k][j*BOX_SIZE+l].val;
			        //if ( !(c3 = isValid( v )) )
                    c3 = isValid(ref v);
                    if (c3 == -1)
				        return( -1 );
		        }

	        return( (c1+c2+c3) == BOX_SIZE ? 1 : -1 );
        }

        //Depth First Search
        private int findSolution_DFS() {
            int i, j, k;
            int check;
            Rectangle slot;
            
            check = isSolved();
            if (check == 1)
                return 1;
            else if (check == 0)
                return 0;

            for (i = 0; i < GRID_SIZE; i++)
                for (j = 0; j < GRID_SIZE; j++) {
                    //if (!grid[i][j].val) {
                    if (GetSlotValue(PuzzleGrid.Children.Cast<Rectangle>().ElementAt(i + j * GRID_SIZE) as Rectangle) == 0)
                    {
                        for (k = 1; k <= GRID_SIZE; k++) {
                            //grid[i][j].val = k;
                            //slot = PuzzleGrid.Children.ElementAt(i + j * GRID_SIZE) as Rectangle;
                            ////_rect.Dispatcher.BeginInvoke(delegate() {
                                SetSlotValue(i, j, k);
                            ////});
                            //if (checkCount > 100) {
                            //    return 0;
                            //}
                            //System.Threading.Thread.Sleep(100);
                            if (findSolution_DFS() == 1)
                                return 1;
                        }
                        //grid[i][j].val = 0;
                        //slot = PuzzleGrid.Children.ElementAt(i + j * GRID_SIZE) as Rectangle;
                        this.Visibility = Visibility.Visible;
                        SetSlotValue(i, j, 0);
                        return 0;
                    }
                }

            return (0);
        }

        public static void SolvePuzzle() {
            //Thread myThread = new Thread(Main.findSolution_DFS);
            Main.findSolution_DFS();
        }

        public static void SolvePuzzle2() {
            //Main.Dispatcher.BeginInvoke(delegate() {
            
                //Main.findSolution_DFS();

            //SolidColorBrush[] m_Colors2 = new SolidColorBrush[GRID_SIZE + 1];

            //// This array will be used to color the grid based on value
            //m_Colors2[0] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)); // The blank
            //m_Colors2[1] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
            //m_Colors2[2] = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            //m_Colors2[3] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            //m_Colors2[4] = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            //m_Colors2[5] = new SolidColorBrush(Color.FromArgb(255, 0, 255, 255));
            //m_Colors2[6] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 255));

            //m_Colors2[7] = new SolidColorBrush(Color.FromArgb(255, 125, 125, 125));
            //m_Colors2[8] = new SolidColorBrush(Color.FromArgb(255, 125, 0, 255));
            //m_Colors2[9] = new SolidColorBrush(Color.FromArgb(255, 0, 125, 255));

                while (true) {
                    //NOTE: The dispatcher is going to invoke the delegate code on the UI thread.
                    ////_rect.Dispatcher.BeginInvoke(delegate() {
                    for (int k = 1; k <= GRID_SIZE; k++) {
                //        //grid[i][j].val = k;
                //        //slot = PuzzleGrid.Children.ElementAt(i + j * GRID_SIZE) as Rectangle;
                _rect.Fill =  m_Colors[k];
                        
                    }
                    ////});
                    System.Threading.Thread.Sleep(10000);
                }
            
            //findSolution_DFS();new SolidColorBrush(Color.FromArgb(255, 125, 125, 125)); //
        }
        

        #region events
        [PreEmptive.Attributes.Feature("NewPuzzle")]
        private void NewPuzzleBtn_Click(object sender, RoutedEventArgs e) {
            // Init the colors if not already done so
            if (m_Colors == null) {
                InitializeColors();
            }                        
            
//            LoadPuzzle("......5.....89562....2..4.3..6.....847.....968.....1..6.2..1....13527.....7......");
           
            //NOTE: This worked, but I think it's basically by accident.
            // LoadPuzzle(".47258369258369471369471582471582693582693714693714825714825936825936147936147258");
            //TODO: This doesn't work.  It just sets all the blanks to 1.
            LoadPuzzle(".472583692583694.13694715824715826935826937146.3.14825714825936825.36147936147258");
        }

        private void SolveBtn_Click(object sender, RoutedEventArgs e) {
            // Init the colors if not already done so
            if (m_Colors == null) {
                InitializeColors();
            }

            _thread1 = new Thread(SolvePuzzle); 
            _thread1.Start();
            //Main.Dispatcher.BeginInvoke( (Action)delegate()
            //{
            //    SolvePuzzle();
            //});
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Format(System.Globalization.CultureInfo.CurrentUICulture, "We are inside {0}.button1_Click()", this.ToString()),
                            "Sudoku");

        }

       

        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }

        

        private void slot_Click(object sender, RoutedEventArgs e)
        {
            Rectangle slot = sender as Rectangle;
            int position = Convert.ToInt32(slot.Name.Replace("slot",""));
            SetSlotValue(position, GetSlotValue(slot) + 1); 
        }

        #endregion
    }
}