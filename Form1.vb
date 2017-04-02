Imports System.IO

Friend Class wndOkno
    Friend jednostki As Jednostka()
    Friend img1 As Bitmap = New Bitmap(1, 1)
    Friend img2 As Bitmap = New Bitmap(1, 1)
    Friend gr1 As Graphics = Graphics.FromImage(img1)
    Friend gr2 As Graphics = Graphics.FromImage(img2)
    Private panel_zazn As PanelZaznaczony = PanelZaznaczony.Nic
    Private zdarzenia As Boolean = False
    Private zapisany As Boolean = True
    Private zamknij As Boolean = False

    'Rysowanie
    Friend czy_img1 As Boolean = False
    Friend promien As Integer = 0
    Friend srednica As Integer = 0
    Private co_zazn As CoZaznaczone = CoZaznaczone.Nic
    Private przesun_wsp As Point
    Private zaznaczone As Cialo
    Private zaznaczone_id As Integer = -1
    Private wektor_rysuj As Boolean = False
    Private Czcionka As New Font("Arial", 15, FontStyle.Regular, GraphicsUnit.Pixel)
    Private CialoPrKolor As New SolidBrush(Color.FromArgb(255, 255, 51, 51))
    Private CialoKolor As New SolidBrush(Color.FromArgb(255, 54, 51, 255))
    Private CialoZaznKolor As New SolidBrush(Color.FromArgb(255, 0, 255, 72))
    Private WektorKolor As New Pen(Color.Black, 3)

#Region "Inicjalizacja"

    Private Sub wndOkno_Load() Handles MyBase.Load
        Okno = Me
        zdarzenia = False
        pctSkala.Image = My.Resources.Plus
        pctCialoProbne.Image = My.Resources.Plus
        pctPoleCentralne.Image = My.Resources.Plus
        pctPoleJednorodne.Image = My.Resources.Plus

        'Utworzenie jednostek
        ReDim jednostki(11)
        jednostki(0) = New Jednostka("G", 1000000000)
        jednostki(1) = New Jednostka("M", 1000000)
        jednostki(2) = New Jednostka("k", 1000)
        jednostki(3) = New Jednostka("h", 100)
        jednostki(4) = New Jednostka("da", 10)
        jednostki(5) = New Jednostka("", 1)
        jednostki(6) = New Jednostka("d", 0.1)
        jednostki(7) = New Jednostka("c", 0.01)
        jednostki(8) = New Jednostka("m", 0.001)
        jednostki(9) = New Jednostka("µ", 0.000001)
        jednostki(10) = New Jednostka("n", 0.000000001)
        jednostki(11) = New Jednostka("p", 0.000000000001)

        'Jednostki skali
        UtworzJednostki("m", cboSkalaJednostka)
        UtworzJednostki("g", cboCialoPrMasa)
        UtworzJednostki("C", cboCialoPrLadunek)
        UtworzJednostki("g", cboPoleCentMasa)
        UtworzJednostki("C", cboPoleCentLadunek)

        'Otwarcie pliku
        Dim p As String = ""
        For i As Integer = 0 To My.Application.CommandLineArgs.Count - 1
            If My.Application.CommandLineArgs.Item(i).EndsWith(".pole") Then
                p = My.Application.CommandLineArgs.Item(i)
            End If
        Next

        If p = "" Then
            NowyPlik_Okno()
        Else
            OtworzPlik_Okno(p)
        End If

        zdarzenia = True
        zapisany = True
    End Sub

    Private Sub UtworzJednostki(ByVal nazwa As String, ByRef cbo As ComboBox)
        Dim j As Jednostka

        For i As Integer = 0 To 11
            j = New Jednostka(jednostki(i).Przedrostek & nazwa, jednostki(i).Mnoznik)
            cbo.Items.Add(j)
        Next
    End Sub

    Private Sub PromienCiala(ByVal gr As Graphics)
        Dim rozm As SizeF = gr.MeasureString("c99", Czcionka)
        promien = CInt(Math.Sqrt(0.25 * rozm.Width * rozm.Width + 0.25 * rozm.Height * rozm.Height))
        srednica = promien << 1
    End Sub

#End Region 'Inicjalizacja

#Region "Okno"

    Private Sub wndOkno_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Animacja Then
            zamknij = True
            Koniec = True
            e.Cancel = True
        Else
            If zapisany = False Then
                If CzyAnulujPlik() Then e.Cancel = True
            End If
        End If
    End Sub

    Private Sub wndOkno_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Dim t As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
            For i As Integer = 0 To t.Length - 1

                If t(i).EndsWith(".pole") Then
                    e.Effect = DragDropEffects.Copy
                    Exit Sub
                End If

            Next
            e.Effect = DragDropEffects.None
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub wndOkno_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        Dim p As String = ""
        Dim t As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())

        For i As Integer = 0 To t.Length - 1
            If t(i).EndsWith(".pole") Then p = t(i)
        Next

        If zapisany = False Then
            If CzyAnulujPlik() Then Exit Sub
        End If

        zdarzenia = False
        OtworzPlik_Okno(p)
        zdarzenia = True
        zapisany = True
    End Sub

#End Region 'Okno

#Region "Pliki"

    Private Sub UstawKontrolki()
        'Skala rysunku
        txtSkalaJednostka.Text = Skala0.ToString

        For i As Integer = 0 To cboSkalaJednostka.Items.Count - 1
            If CType(cboSkalaJednostka.Items(i), Jednostka).Przedrostek = SkalaJedn.Przedrostek Then
                cboSkalaJednostka.SelectedItem = cboSkalaJednostka.Items(i)
                Exit For
            End If
        Next

        'Ciało próbne
        txtCialoPrX.Text = CialoProbne.X0.ToString
        txtCialoPrY.Text = CialoProbne.Y0.ToString
        txtCialoPrv0.Text = Math.Sqrt(Wektor.X0 ^ 2 + Wektor.Y0 ^ 2).ToString
        txtCialoPrv0x.Text = Wektor.X0.ToString
        txtCialoPrv0y.Text = Wektor.Y0.ToString

        Select Case Rodzaj
            Case RodzajCiala.Proton : rbCialoPrProton.Checked = True
            Case RodzajCiala.Elektron : rbCialoPrElektron.Checked = True
            Case RodzajCiala.Inne : rbCialoPrInne.Checked = True
        End Select

        txtCialoPrMasa.Text = CialoProbne.Masa0.ToString
        For i As Integer = 0 To cboCialoPrMasa.Items.Count - 1
            If CType(cboCialoPrMasa.Items(i), Jednostka).Przedrostek = CialoProbne.MasaJedn.Przedrostek Then
                cboCialoPrMasa.SelectedItem = cboCialoPrMasa.Items(i)
                Exit For
            End If
        Next

        txtCialoPrLadunek.Text = CialoProbne.Ladunek0.ToString
        For i As Integer = 0 To cboCialoPrLadunek.Items.Count - 1
            If CType(cboCialoPrLadunek.Items(i), Jednostka).Przedrostek = CialoProbne.LadunekJedn.Przedrostek Then
                cboCialoPrLadunek.SelectedItem = cboCialoPrLadunek.Items(i)
                Exit For
            End If
        Next

        'Pole centralne
        lvCiala.SelectedItems.Clear()
        lvCiala.Items.Clear()
        Dim lvi As ListViewItem
        Dim el(3) As String

        If Ciala IsNot Nothing Then
            For i As Integer = 0 To Ciala.Length - 1
                If Ciala(i) Is Nothing Then Continue For

                el(0) = "c" & (i + 1).ToString
                el(1) = Ciala(i).Masa0.ToString & " " & Ciala(i).MasaJedn.Przedrostek
                el(2) = Ciala(i).Ladunek0.ToString & " " & Ciala(i).LadunekJedn.Przedrostek
                el(3) = i.ToString
                lvi = New ListViewItem(el)

                lvCiala.Items.Add(lvi)
                Ciala(i).ListViewId = lvi.Index
            Next
        End If

        CzyscPoleCent()
        ZaznaczCentMasa("kg")
        ZaznaczCentLadunek("C")

        'Pole jednorodne
        If Gora Then cbPoleJednGrawGora.Checked = True Else cbPoleJednGrawGora.Checked = False
        If Dol Then cbPoleJednGrawDol.Checked = True Else cbPoleJednGrawDol.Checked = False
        If Lewo Then cbPoleJednGrawLewo.Checked = True Else cbPoleJednGrawLewo.Checked = False
        If Prawo Then cbPoleJednGrawPrawo.Checked = True Else cbPoleJednGrawPrawo.Checked = False

        txtPoleJednGrawGora.Text = GoraIle.ToString
        txtPoleJednGrawDol.Text = DolIle.ToString
        txtPoleJednGrawLewo.Text = LewoIle.ToString
        txtPoleJednGrawPrawo.Text = PrawoIle.ToString

        If GoraDol Then cbPoleJednElGora.Checked = True Else cbPoleJednElGora.Checked = False
        If LewoPrawo Then cbPoleJednElLewo.Checked = True Else cbPoleJednElLewo.Checked = False

        If GoraPlus Then rbPoleJednElGoraPlus.Checked = True Else rbPoleJednElGoraMinus.Checked = True
        If LewoPlus Then rbPoleJednElLewoPlus.Checked = True Else rbPoleJednElLewoMinus.Checked = True

        txtPoleJednElGoraE.Text = GoraDolIle.ToString
        txtPoleJednElLewoE.Text = LewoPrawoIle.ToString

        Maluj()
    End Sub

    Private Sub NowyPlik_Okno()
        Plik = ""
        Sciezka = ""
        CzyscZmienne()
        UstawKontrolki()
    End Sub

    Private Sub OtworzPlik_Okno(ByVal SciezkaPliku As String)
        If SciezkaPliku = "" Then
            dlgOtworz.FileName = ""
            If dlgOtworz.ShowDialog = Windows.Forms.DialogResult.Cancel Then Exit Sub
            Sciezka = dlgOtworz.FileName
        Else
            Sciezka = SciezkaPliku
        End If

        Dim t As String() = Sciezka.Split(Path.PathSeparator)
        Plik = t(t.Length - 1)

        CzyscZmienne()
        OtworzPlik()
        UstawKontrolki()
    End Sub

    Private Function CzyAnulujPlik() As Boolean
        Select Case MessageBox.Show("Zapisać zmiany w pliku " & Plik & "?", "Zapisywanie pliku", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)
            Case Windows.Forms.DialogResult.Yes
                ZapiszPlik(False)
                Return False
            Case Windows.Forms.DialogResult.No
                Return False
            Case Windows.Forms.DialogResult.Cancel
                Return True
        End Select
    End Function

#End Region 'Pliki

#Region "Malowanie"

    Private Sub pnlObraz_Resize() Handles pnlObraz.Resize
        If Animacja Then Exit Sub
        If pctObraz.Width = 0 Or pctObraz.Height = 0 Then Exit Sub
        WysY = pctObraz.Height
        SzerX = pctObraz.Width
        Maluj()
    End Sub

    'Rysuje na pctObraz po zmianie parametrów wyświetlanych elementów
    Friend Sub Maluj()

        'Przygotowanie obrazu
        Dim img As Bitmap
        Dim gr As Graphics

        If czy_img1 Then
            'Sprawdzenie rozmiaru
            If (img2.Width <> pctObraz.Width) Or (img2.Height <> pctObraz.Height) Then
                gr2.Dispose()
                img2.Dispose()

                img2 = New Bitmap(pctObraz.Width, pctObraz.Height)
                gr2 = Graphics.FromImage(img2)
            End If

            img = img2
            gr = gr2
            If Animacja = False Then czy_img1 = False
        Else
            'Sprawdzenie rozmiaru
            If (img1.Width <> pctObraz.Width) Or (img1.Height <> pctObraz.Height) Then
                gr1.Dispose()
                img1.Dispose()

                img1 = New Bitmap(pctObraz.Width, pctObraz.Height)
                gr1 = Graphics.FromImage(img1)
            End If

            img = img1
            gr = gr1
            If Animacja = False Then czy_img1 = True
        End If

        If promien = 0 Then PromienCiala(gr)

        'Rysowanie
        gr.Clear(Color.White)

        'Ciała
        If Ciala IsNot Nothing Then

            If panel_zazn = PanelZaznaczony.PoleCentralne Then

                'Ciała niezaznaczone
                For i As Integer = 0 To Ciala.Length - 1
                    If Ciala(i) IsNot Nothing Then

                        If Ciala(i) IsNot zaznaczone Then

                            gr.FillEllipse(CialoKolor, Ciala(i).X - promien, WysY - Ciala(i).Y - promien, srednica, srednica)
                            If Ciala(i).TekstX = 0 Then
                                Dim rozm As SizeF = gr.MeasureString("c" & (i + 1), Czcionka)
                                Ciala(i).TekstX = CInt(-(rozm.Width / 2))
                                Ciala(i).TekstY = CInt(-(rozm.Height / 2))
                            End If
                            gr.DrawString("c" & (i + 1).ToString, Czcionka, Brushes.White, Ciala(i).X + Ciala(i).TekstX, WysY - Ciala(i).Y + Ciala(i).TekstY)

                        End If

                    End If
                Next

                'Ciało zaznaczone
                If zaznaczone IsNot Nothing Then
                    gr.FillEllipse(CialoZaznKolor, zaznaczone.X - promien, WysY - zaznaczone.Y - promien, srednica, srednica)

                    If zaznaczone.TekstX = 0 Then
                        Dim rozm As SizeF = gr.MeasureString("c" & (zaznaczone_id + 1), Czcionka)
                        zaznaczone.TekstX = CInt(-(rozm.Width / 2))
                        zaznaczone.TekstY = CInt(-(rozm.Height / 2))
                    End If
                    gr.DrawString("c" & (zaznaczone_id + 1).ToString, Czcionka, Brushes.White, zaznaczone.X + zaznaczone.TekstX, WysY - zaznaczone.Y + zaznaczone.TekstY)
                End If

            Else

                'Wszystkie ciała
                For i As Integer = 0 To Ciala.Length - 1
                    If Ciala(i) IsNot Nothing Then

                        gr.FillEllipse(CialoKolor, Ciala(i).X - promien, WysY - Ciala(i).Y - promien, srednica, srednica)
                        If Ciala(i).TekstX = 0 Then
                            Dim rozm As SizeF = gr.MeasureString("c" & (i + 1), Czcionka)
                            Ciala(i).TekstX = CInt(-(rozm.Width / 2))
                            Ciala(i).TekstY = CInt(-(rozm.Height / 2))
                        End If
                        gr.DrawString("c" & (i + 1).ToString, Czcionka, Brushes.White, Ciala(i).X + Ciala(i).TekstX, WysY - Ciala(i).Y + Ciala(i).TekstY)

                    End If
                Next

            End If


        End If

        'Ciało próbne
        If Animacja = False Then gr.FillEllipse(CialoPrKolor, CialoProbne.X - promien, WysY - CialoProbne.Y - promien, srednica, srednica)

        'Wektor
        If (Animacja = False) And wektor_rysuj Then    'Wszystko we współrzędnych ekranu

            Dim w As New Point(Wektor.X, -Wektor.Y)     'Współrzędne wektora
            Dim A As New PointF(CialoProbne.X, WysY - CialoProbne.Y)     'Współrzędne ciała próbnego i początku wektora
            Dim B As New PointF(w.X + A.X, w.Y + A.Y)    'Koniec wektora
            Dim C As PointF  'Punkt na prostej k w odległści 8px od wektora
            Dim D As PointF  'Punkt przecięcia prostej k i wektora w odległości 20px od końca wektora
            Dim E As PointF  'To samo co C tylko z drugiej strony wektora
            'Wektor: odcinek AB, linia do narysowania: AD, grot wektora: trójkąt BCE
            Dim v As Double = Math.Sqrt(w.X ^ 2 + w.Y ^ 2) 'Długość wektora
            Dim u1 As PointF    'Wektor jednostkowy w * 20 (BD)
            Dim k As Double     'Współczynnik kierunkowy prostej prostopadłej do wektora
            Dim P1 As PointF    'Punkt na prostej k
            Dim z As PointF     'Wektor PP1
            Dim z1 As Double    'Długość wektora z

            If v > 30 Then  'Rysuj linię ze strzałką

                If w.X = 0 Then 'Kierunek pionowy

                    If w.Y > 0 Then 'W dół
                        C = New PointF(B.X + 8, B.Y - 20)
                        D = New PointF(B.X, B.Y - 20)
                        E = New PointF(B.X - 8, B.Y - 20)
                    Else    'W górę
                        C = New PointF(B.X - 8, B.Y + 20)
                        D = New PointF(B.X, B.Y + 20)
                        E = New PointF(B.X + 8, B.Y + 20)
                    End If

                End If

                If w.Y = 0 Then 'Kierunek poziomy

                    If w.X > 0 Then 'W prawo
                        C = New PointF(B.X - 20, B.Y - 8)
                        D = New PointF(B.X - 20, B.Y)
                        E = New PointF(B.X - 20, B.Y + 8)
                    Else    'W lewo
                        C = New PointF(B.X + 20, B.Y + 8)
                        D = New PointF(B.X + 20, B.Y)
                        E = New PointF(B.X + 20, B.Y - 8)
                    End If

                End If

                'Ukośnie
                If (w.X <> 0) And (w.Y <> 0) Then
                    u1 = New PointF(CSng(20 * w.X / v), CSng(20 * w.Y / v))
                    D = New PointF(B.X - u1.X, B.Y - u1.Y)  'Punkt przecięcia wektora i prostej
                    k = (B.Y - A.Y) / (B.X - A.X)
                    k = -(1 / k)    'Współczynnik kierunkowy prostej
                    P1 = New PointF(D.X + 1, CSng(D.Y + k))
                    z = New PointF(P1.X - D.X, P1.Y - D.Y)  'Wektor na prostej
                    z1 = Math.Sqrt(z.X ^ 2 + z.Y ^ 2)
                    z = New PointF(CSng(8 * z.X / z1), CSng(8 * z.Y / z1))    'Wektor o długości 8 na prostej
                    C = New PointF(D.X - z.X, D.Y - z.Y)
                    E = New PointF(D.X + z.X, D.Y + z.Y)
                End If

                'Narysowanie
                Dim pkt As PointF() = {B, C, E}
                gr.DrawLine(WektorKolor, A, D)
                gr.FillPolygon(Brushes.Black, pkt)

            Else    'Wektor za krótki, narysuj tylko linię
                gr.DrawLine(WektorKolor, A, B)
            End If

        End If

        'Podziałka
        'Oś X
        Dim roz As SizeF
        Dim t As String

        For i As Integer = 100 To img.Width - 75 Step 100
            t = (i * Skala0).ToString
            roz = gr.MeasureString(t, Czcionka)
            gr.DrawLine(Pens.Black, i, img.Height, i, img.Height - 15)
            gr.DrawString(t, Czcionka, Brushes.Black, i - (roz.Width / 2), img.Height - 15 - roz.Height)
        Next

        t = "x [" & SkalaJedn.Przedrostek & "]"
        roz = gr.MeasureString(t, Czcionka)
        gr.DrawString(t, Czcionka, Brushes.Black, img.Width - roz.Width - 10, img.Height - roz.Height - 15)

        'Oś Y
        Dim yp As Integer = 100
        For i As Integer = img.Height - 100 To 75 Step -100
            t = (yp * Skala0).ToString
            roz = gr.MeasureString(t, Czcionka)
            gr.DrawString(t, Czcionka, Brushes.Black, 15, i - (roz.Height / 2))
            gr.DrawLine(Pens.Black, 0, i, 15, i)
            yp += 100
        Next

        gr.DrawString("y [" & SkalaJedn.Przedrostek & "]", Czcionka, Brushes.Black, 15, 10)

        'Wyświetlenie
        If Animacja = False Then pctObraz.Image = img
    End Sub

#End Region 'Malowanie

#Region "Obsługa paneli"

    Private Sub ZwinPanele()
        If panel_zazn = PanelZaznaczony.Nic Then Exit Sub

        'Skala
        pnlSkalaDane.Visible = False
        pctSkala.Image = My.Resources.Plus

        'Ciało próbne
        pnlCialoProbne.Location = New Point(3, 31)
        pnlCialoProbneDane.Visible = False
        pctCialoProbne.Image = My.Resources.Plus

        'Pole centralne
        pnlPoleCentralne.Location = New Point(3, 59)
        pnlPoleCentDane.Visible = False
        pctPoleCentralne.Image = My.Resources.Plus

        'Pole jednorodne
        pnlPoleJednorodne.Location = New Point(3, 87)
        pnlPoleJednDane.Visible = False
        pctPoleJednorodne.Image = My.Resources.Plus
    End Sub

    Private Sub pnlSkala_Click() Handles pnlSkala.Click, lblSkala.Click, pctSkala.Click
        wektor_rysuj = False
        ZwinPanele()

        If panel_zazn <> PanelZaznaczony.Skala Then 'Pokazuje kliknięty panel (Skala)
            pctSkala.Image = My.Resources.Minus
            pnlCialoProbne.Location = New Point(3, 31 + pnlSkalaDane.Height)
            pnlPoleCentralne.Location = New Point(3, 59 + pnlSkalaDane.Height)
            pnlPoleJednorodne.Location = New Point(3, 87 + pnlSkalaDane.Height)

            pnlSkalaDane.Location = New Point(0, pnlSkala.Bottom)
            pnlSkalaDane.Visible = True
            panel_zazn = PanelZaznaczony.Skala
        Else
            panel_zazn = PanelZaznaczony.Nic
        End If

        Maluj()
    End Sub

    Private Sub pnlCialoProbne_Click() Handles pnlCialoProbne.Click, lblCialoProbne.Click, pctCialoProbne.Click
        ZwinPanele()

        If panel_zazn <> PanelZaznaczony.CialoProbne Then 'Pokazuje kliknięty panel (Ciało próbne)
            pctCialoProbne.Image = My.Resources.Minus
            pnlPoleCentralne.Location = New Point(3, 59 + pnlCialoProbneDane.Height)
            pnlPoleJednorodne.Location = New Point(3, 87 + pnlCialoProbneDane.Height)

            pnlCialoProbneDane.Location = New Point(0, pnlCialoProbne.Bottom)
            pnlCialoProbneDane.Visible = True
            panel_zazn = PanelZaznaczony.CialoProbne
            wektor_rysuj = True
        Else
            panel_zazn = PanelZaznaczony.Nic
            wektor_rysuj = False
        End If

        Maluj()
    End Sub

    Private Sub pnlPoleCentralne_Click() Handles pnlPoleCentralne.Click, lblPoleCentralne.Click, pctPoleCentralne.Click
        wektor_rysuj = False
        ZwinPanele()

        If panel_zazn <> PanelZaznaczony.PoleCentralne Then 'Pokazuje kliknięty panel (Pole centralne)
            pctPoleCentralne.Image = My.Resources.Minus
            pnlPoleJednorodne.Location = New Point(3, 87 + pnlPoleCentDane.Height)

            pnlPoleCentDane.Location = New Point(0, pnlPoleCentralne.Bottom)
            pnlPoleCentDane.Visible = True
            panel_zazn = PanelZaznaczony.PoleCentralne
        Else
            panel_zazn = PanelZaznaczony.Nic
        End If

        Maluj()
    End Sub

    Private Sub pnlPoleJednorosne_Click() Handles pnlPoleJednorodne.Click, lblPoleJednorodne.Click, pctPoleJednorodne.Click
        wektor_rysuj = False
        ZwinPanele()

        If panel_zazn <> PanelZaznaczony.PoleJednorodne Then 'Pokazuje kliknięty panel (Pole jednorodne)
            pctPoleJednorodne.Image = My.Resources.Minus

            pnlPoleJednDane.Location = New Point(0, pnlPoleJednorodne.Bottom)
            pnlPoleJednDane.Visible = True
            panel_zazn = PanelZaznaczony.PoleJednorodne
        Else
            panel_zazn = PanelZaznaczony.Nic
        End If

        Maluj()
    End Sub

#End Region 'Obsługa paneli

#Region "Animacje"

    Private Sub PrzygotujImg1()
        Dim pimg1 As Integer
        Dim ptlo1 As Integer
        Dim pcpr As Integer
        Dim rct As New Rectangle
        Dim jpocz As Integer
        Dim jkon As Integer
        Dim ipocz As Integer
        Dim ikon As Integer
        Dim m As Integer
        Dim n As Integer

        img1_b = New BitmapBytesRGB24(img1)

        If (CialoProbne.X > -Okno.promien) And (CialoProbne.X < (SzerX + Okno.promien)) And (CialoProbne.Y > -Okno.promien) And (CialoProbne.Y < (WysY + Okno.promien)) Then    'Czy ciało znajduje się na obszarze rysunku

            rct = New Rectangle(CialoProbne.X - Okno.promien, WysY - CialoProbne.Y - Okno.promien, Okno.srednica, Okno.srednica)
            ipocz = 0
            ikon = Okno.srednica - 1
            jpocz = 0
            jkon = 3 * (Okno.srednica - 1)
            m = 0
            n = 0

            If rct.X < 0 Then
                jpocz = 3 * (-rct.X - 1)
                rct.X = 0
            End If

            If (rct.X + Okno.srednica) > SzerX Then
                jkon = 3 * (SzerX - rct.X - 1)
                n = 3 * (Okno.srednica - SzerX + rct.X)
                rct.X = SzerX - Okno.srednica
            End If

            If rct.Y < 0 Then
                ipocz = -rct.Y
                rct.Y = 0
            End If

            If (rct.Y + Okno.srednica) > WysY Then
                ikon = WysY - rct.Y
                m = WysY - (WysY - Okno.srednica) - (WysY - rct.Y) - 1
                rct.Y = WysY - Okno.srednica
            End If

            img1_b.LockBitmap(rct)

            'Skopiowanie tła
            For i As Integer = 0 To Okno.srednica - 1
                pimg1 = i * img1_b.RowSizeBytes
                ptlo1 = i * img1_tlo_b.RowSizeBytes

                For j As Integer = 0 To 3 * (Okno.srednica - 1)
                    img1_tlo_b.ImageBytes(ptlo1) = img1_b.ImageBytes(pimg1)
                    pimg1 += 1
                    ptlo1 += 1
                Next

            Next

            'Skopiowanie ciała
            For i As Integer = ipocz To ikon
                pimg1 = m * img1_b.RowSizeBytes + n
                pcpr = i * cialo_b.RowSizeBytes + jpocz

                For j As Integer = jpocz To jkon
                    If cialo_b.ImageBytes(pcpr) <> 255 Then img1_b.ImageBytes(pimg1) = cialo_b.ImageBytes(pcpr)
                    pcpr += 1
                    pimg1 += 1
                Next

                m += 1
            Next

            img1_b.UnlockBitmap()

        End If

        pctObraz.Image = img1
        czy_img1 = True
    End Sub

    Private Sub PrzygotujImg2()
        Dim pimg2 As Integer
        Dim ptlo2 As Integer
        Dim pcpr As Integer
        Dim rct As New Rectangle
        Dim jpocz As Integer
        Dim jkon As Integer
        Dim ipocz As Integer
        Dim ikon As Integer
        Dim m As Integer
        Dim n As Integer

        img2_b = New BitmapBytesRGB24(img2)

        If (CialoProbne.X > -Okno.promien) And (CialoProbne.X < (SzerX + Okno.promien)) And (CialoProbne.Y > -Okno.promien) And (CialoProbne.Y < (WysY + Okno.promien)) Then    'Czy ciało znajduje się na obszarze rysunku

            rct = New Rectangle(CialoProbne.X - Okno.promien, WysY - CialoProbne.Y - Okno.promien, Okno.srednica, Okno.srednica)
            ipocz = 0
            ikon = Okno.srednica - 1
            jpocz = 0
            jkon = 3 * (Okno.srednica - 1)
            m = 0
            n = 0

            If rct.X < 0 Then
                jpocz = 3 * (-rct.X - 1)
                rct.X = 0
            End If

            If (rct.X + Okno.srednica) > SzerX Then
                jkon = 3 * (SzerX - rct.X - 1)
                n = 3 * (Okno.srednica - SzerX + rct.X)
                rct.X = SzerX - Okno.srednica
            End If

            If rct.Y < 0 Then
                ipocz = -rct.Y
                rct.Y = 0
            End If

            If (rct.Y + Okno.srednica) > WysY Then
                ikon = WysY - rct.Y
                m = WysY - (WysY - Okno.srednica) - (WysY - rct.Y) - 1
                rct.Y = WysY - Okno.srednica
            End If

            img2_b.LockBitmap(rct)

            'Skopiowanie tła
            For i As Integer = 0 To Okno.srednica - 1
                pimg2 = i * img2_b.RowSizeBytes
                ptlo2 = i * img2_tlo_b.RowSizeBytes

                For j As Integer = 0 To 3 * (Okno.srednica - 1)
                    img2_tlo_b.ImageBytes(ptlo2) = img2_b.ImageBytes(pimg2)
                    pimg2 += 1
                    ptlo2 += 1
                Next

            Next

            'Skopiowanie ciała
            For i As Integer = ipocz To ikon
                pimg2 = m * img2_b.RowSizeBytes + n
                pcpr = i * cialo_b.RowSizeBytes + jpocz

                For j As Integer = jpocz To jkon
                    If cialo_b.ImageBytes(pcpr) <> 255 Then img2_b.ImageBytes(pimg2) = cialo_b.ImageBytes(pcpr)
                    pcpr += 1
                    pimg2 += 1
                Next

                m += 1
            Next

            img2_b.UnlockBitmap()

        End If

        pctObraz.Image = img2
        czy_img1 = False
    End Sub

    Private Sub btnStart_Click() Handles btnStart.Click
        If Animacja Then
            Pauza = Not Pauza
            Exit Sub
        End If

        'Przygotowanie
        Koniec = False
        Animacja = True
        zamknij = False
        btnStart.Text = "Pauza"
        btnStop.Enabled = True
        mnuMenu.Enabled = False
        MaximizeBox = False
        MaximumSize = Size
        MinimumSize = Size
        ZwinPanele()
        panel_zazn = PanelZaznaczony.Nic
        wektor_rysuj = False
        pnlSkala.Enabled = False
        pnlCialoProbne.Enabled = False
        pnlPoleCentralne.Enabled = False
        pnlPoleJednorodne.Enabled = False

        If cialo_rys Is Nothing Then
            cialo_rys = New Bitmap(srednica, srednica)
            Dim gr As Graphics = Graphics.FromImage(cialo_rys)
            gr.Clear(Color.White)
            gr.FillEllipse(CialoPrKolor, 0, 0, srednica - 1, srednica - 1)
            gr.Dispose()
            cialo_b = New BitmapBytesRGB24(cialo_rys)
            cialo_b.LockBitmap(New Rectangle(0, 0, srednica, srednica))

            img1_tlo = New Bitmap(srednica, srednica)
            img1_tlo_b = New BitmapBytesRGB24(img1_tlo)
            img1_tlo_b.LockBitmap(New Rectangle(0, 0, srednica, srednica))
            img2_tlo = New Bitmap(srednica, srednica)
            img2_tlo_b = New BitmapBytesRGB24(img2_tlo)
            img2_tlo_b.LockBitmap(New Rectangle(0, 0, srednica, srednica))
        End If

        'Skopiuj tło pierwszego obrazu
        Maluj()
        If czy_img1 Then PrzygotujImg2() Else PrzygotujImg1()

        'Tło drugiego obrazu
        Maluj()
        If czy_img1 Then PrzygotujImg2() Else PrzygotujImg1()

        Dim t As New Threading.Thread(AddressOf Animuj)
        t.Start()
    End Sub

    Private Sub btnStop_Click() Handles btnStop.Click
        Pauza = False
        Koniec = True
    End Sub

#End Region 'Animacje

#Region "Menu"

    Private Sub mnuNowy_Click() Handles mnuNowy.Click
        If zapisany = False Then
            If CzyAnulujPlik() Then Exit Sub
        End If

        zdarzenia = False
        NowyPlik_Okno()
        zdarzenia = True
        zapisany = True
    End Sub

    Private Sub mnuOtwórz_Click() Handles mnuOtwórz.Click
        If zapisany = False Then
            If CzyAnulujPlik() Then Exit Sub
        End If

        zdarzenia = False
        OtworzPlik_Okno("")
        zdarzenia = True
        zapisany = True
    End Sub

    Private Sub mnuZapisz_Click() Handles mnuZapisz.Click
        If zapisany Then Exit Sub
        ZapiszPlik(False)
        zapisany = True
    End Sub

    Private Sub mnuZapiszJako_Click() Handles mnuZapiszJako.Click
        ZapiszPlik(True)
        zapisany = True
    End Sub

    Private Sub mnuPokażFPS_Click() Handles mnuPokażFPS.Click
        PokazFps = Not PokazFps
    End Sub

#End Region 'Menu

#Region "Zaznaczanie i edycja kontrolek"
    '--------------------------------Sprawdzanie poprawności----------------------
    'Sprawdza, czy wartość w polu tekstowym jest dodatnia
    Private Function SprDodatnie(ByRef pole As TextBox, ByRef poprawne As Boolean) As Double
        Dim d As Double
        poprawne = False

        If pole.Text = "" Then
            poprawne = False
            erpBledy.SetError(pole, BLPUSTE)
            Return 0
        End If

        Try
            d = CDbl(pole.Text)
        Catch
            poprawne = False
            erpBledy.SetError(pole, BLNIEPOPRAWNE)
            Return 0
        End Try

        If d <= 0 Then
            poprawne = False
            erpBledy.SetError(pole, BLUJEMNEZERO)
            Return 0
        End If

        poprawne = True
        erpBledy.SetError(pole, "")
        Return d
    End Function

    'Sprawdza, czy wartość w polu jest poprawna; jeśli pole jest puste, wartość wynosi 0
    Friend Function SprPopr(ByRef pole As TextBox, ByRef poprawne As Boolean) As Double
        poprawne = False

        If pole.Text = "" Then
            poprawne = True
            erpBledy.SetError(pole, "")
            Return 0
        End If

        Dim d As Double

        Try
            d = Double.Parse(pole.Text)
        Catch
            poprawne = False
            erpBledy.SetError(pole, BLNIEPOPRAWNE)
            Return 0
        End Try

        poprawne = True
        erpBledy.SetError(pole, "")
        Return d
    End Function

    'Sprawdza, czy wartość w polu jest dodatnia lub wynosi 0; jeśli pole jest puste, wartość wynosi 0
    Friend Function SprDodatnieZero(ByRef pole As TextBox, ByRef poprawne As Boolean) As Double
        poprawne = False

        If pole.Text = "" Then
            poprawne = True
            erpBledy.SetError(pole, "")
            Return 0
        End If

        Dim d As Double

        Try
            d = Double.Parse(pole.Text)
        Catch
            poprawne = False
            erpBledy.SetError(pole, BLNIEPOPRAWNE)
            Return 0
        End Try

        If d < 0 Then
            poprawne = False
            erpBledy.SetError(pole, BLUJEMNE)
            Return 0
        End If

        poprawne = True
        erpBledy.SetError(pole, "")
        Return d
    End Function

    '--------------------------------Skala----------------------------------------
    Private Sub ObliczWspolrzedneCial()
        If Ciala IsNot Nothing Then
            For i As Integer = 0 To Ciala.Length - 1
                If Ciala(i) IsNot Nothing Then
                    Ciala(i).Xm = Ciala(i).X * Skala
                    Ciala(i).Ym = Ciala(i).Y * Skala
                    Ciala(i).X0 = Ciala(i).X * Skala0
                    Ciala(i).Y0 = Ciala(i).Y * Skala0
                End If
            Next
        End If

        CialoProbne.Xm = CialoProbne.X * Skala
        CialoProbne.Ym = CialoProbne.Y * Skala
        CialoProbne.X0 = CialoProbne.X * Skala0
        CialoProbne.Y0 = CialoProbne.Y * Skala0

        Wektor.Xm = Wektor.X * Skala
        Wektor.Ym = Wektor.Y * Skala
        Wektor.X0 = Wektor.X * Skala0
        Wektor.Y0 = Wektor.Y * Skala0

        'Wyświetlenie
        zdarzenia = False

        txtCialoPrX.Text = CialoProbne.X0.ToString
        txtCialoPrY.Text = CialoProbne.Y0.ToString
        txtCialoPrv0.Text = Math.Sqrt(Wektor.X0 ^ 2 + Wektor.Y0 ^ 2).ToString
        txtCialoPrv0x.Text = Wektor.X0.ToString
        txtCialoPrv0y.Text = Wektor.Y0.ToString
        If zaznaczone IsNot Nothing Then
            txtPoleCentX.Text = zaznaczone.X0.ToString
            txtPoleCentY.Text = zaznaczone.Y0.ToString
        End If

        zdarzenia = True

    End Sub

    Private Sub txtSkalaJednostka_TextChanged() Handles txtSkalaJednostka.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        Dim d As Double
        Dim popr As Boolean = False
        d = SprDodatnie(txtSkalaJednostka, popr)
        If popr Then
            Skala0 = d
            Skala = Skala0 * SkalaJedn.Mnoznik

            ObliczWspolrzedneCial()
            Maluj()
        End If

    End Sub

    Private Sub cboSkalaJednostka_SelectedIndexChanged() Handles cboSkalaJednostka.SelectedIndexChanged
        If cboSkalaJednostka.SelectedItem Is Nothing Then Exit Sub

        zapisany = False

        Dim e As Jednostka = CType(cboSkalaJednostka.SelectedItem, Jednostka)
        Dim j As String = e.Przedrostek

        lblCialoPrXJednostka.Text = j
        lblCialoPrYJednostka.Text = j
        lblPoleCentXJednostka.Text = j
        lblPoleCentYJednostka.Text = j
        j &= "/s"
        lblCialoPrv0Jednostka.Text = j
        lblCialoPrv0xJednostka.Text = j
        lblCialoPrv0yJednostka.Text = j

        If zdarzenia = False Then Exit Sub

        SkalaJedn = e
        Skala = Skala0 * e.Mnoznik

        ObliczWspolrzedneCial()
        Maluj()
    End Sub

    '----------------------------------------Ciało próbne----------------------------------------------
    Private Sub txtCialoPrX_TextChanged() Handles txtCialoPrX.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtCialoPrX, popr)

        If popr Then
            CialoProbne.X0 = d
            CialoProbne.Xm = d * SkalaJedn.Mnoznik
            CialoProbne.X = CInt(d / Skala0)
            Maluj()
        End If
    End Sub

    Private Sub txtCialoPrY_TextChanged() Handles txtCialoPrY.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtCialoPrY, popr)

        If popr Then
            CialoProbne.Y0 = d
            CialoProbne.Ym = d * SkalaJedn.Mnoznik
            CialoProbne.Y = CInt(d / Skala0)
            Maluj()
        End If
    End Sub

    Private Sub txtCialoPrv0x_TextChanged() Handles txtCialoPrv0x.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtCialoPrv0x, popr)

        If popr Then
            Wektor.X0 = d
            Wektor.Xm = d * SkalaJedn.Mnoznik
            Wektor.X = CInt(d / Skala0)
            txtCialoPrv0.Text = Math.Sqrt(Wektor.X0 ^ 2 + Wektor.Y0 ^ 2).ToString
            Maluj()
        End If
    End Sub

    Private Sub txtCialoPrv0y_TextChanged() Handles txtCialoPrv0y.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtCialoPrv0y, popr)

        If popr Then
            Wektor.Y0 = d
            Wektor.Ym = d * SkalaJedn.Mnoznik
            Wektor.Y = CInt(d / Skala0)
            txtCialoPrv0.Text = Math.Sqrt(Wektor.X0 ^ 2 + Wektor.Y0 ^ 2).ToString
            Maluj()
        End If
    End Sub

    Private Sub rbCialoPrProton_CheckedChanged() Handles rbCialoPrProton.CheckedChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        If rbCialoPrProton.Checked Then Rodzaj = RodzajCiala.Proton
    End Sub

    Private Sub rbCialoPrElektron_CheckedChanged() Handles rbCialoPrElektron.CheckedChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        If rbCialoPrElektron.Checked Then Rodzaj = RodzajCiala.Elektron
    End Sub

    Private Sub rbCialoPrInne_CheckedChanged() Handles rbCialoPrInne.CheckedChanged
        If rbCialoPrInne.Checked Then
            txtCialoPrMasa.Enabled = True
            txtCialoPrLadunek.Enabled = True
            cboCialoPrMasa.Enabled = True
            cboCialoPrLadunek.Enabled = True
        Else
            txtCialoPrMasa.Enabled = False
            txtCialoPrLadunek.Enabled = False
            cboCialoPrMasa.Enabled = False
            cboCialoPrLadunek.Enabled = False
        End If

        If zdarzenia = False Then Exit Sub
        zapisany = False
        If rbCialoPrInne.Checked Then Rodzaj = RodzajCiala.Inne
    End Sub

    Private Sub txtCialoPrMasa_TextChanged() Handles txtCialoPrMasa.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double

        d = SprDodatnie(txtCialoPrMasa, popr)
        If popr Then
            CialoProbne.Masa0 = d
            CialoProbne.Masa = CialoProbne.Masa0 * CialoProbne.MasaJedn.Mnoznik / 1000
        End If
    End Sub

    Private Sub txtCialoPrLadunek_TextChanged() Handles txtCialoPrLadunek.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double

        d = SprPopr(txtCialoPrLadunek, popr)
        If popr Then
            CialoProbne.Ladunek0 = d
            CialoProbne.Ladunek = CialoProbne.Ladunek0 * CialoProbne.LadunekJedn.Mnoznik
        End If
    End Sub

    Private Sub cboCialoPrMasa_SelectedIndexChanged() Handles cboCialoPrMasa.SelectedIndexChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        If cboCialoPrMasa.SelectedItem Is Nothing Then Exit Sub
        CialoProbne.MasaJedn = DirectCast(cboCialoPrMasa.SelectedItem, Jednostka)
        CialoProbne.Masa = CialoProbne.Masa0 * CialoProbne.MasaJedn.Mnoznik / 1000
    End Sub

    Private Sub cboCialoPrLadunek_SelectedIndexChanged() Handles cboCialoPrLadunek.SelectedIndexChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        If cboCialoPrLadunek.SelectedItem Is Nothing Then Exit Sub
        CialoProbne.LadunekJedn = DirectCast(cboCialoPrLadunek.SelectedItem, Jednostka)
        CialoProbne.Ladunek = CialoProbne.Ladunek0 * CialoProbne.LadunekJedn.Mnoznik
    End Sub

    '--------------------------------------------------------Pole centralne---------------------------------------------
    Friend Sub ZaznaczCentMasa(ByVal przedrostek As String)
        Dim j As Jednostka

        For i As Integer = 0 To cboPoleCentMasa.Items.Count - 1
            j = CType(cboPoleCentMasa.Items(i), Jednostka)

            If j.Przedrostek = przedrostek Then
                cboPoleCentMasa.SelectedItem = cboPoleCentMasa.Items(i)
                MasaCentJedn = j
                Exit Sub
            End If

        Next
    End Sub

    Friend Sub ZaznaczCentLadunek(ByVal przedrostek As String)
        Dim j As Jednostka

        For i As Integer = 0 To cboPoleCentLadunek.Items.Count - 1
            j = CType(cboPoleCentLadunek.Items(i), Jednostka)

            If j.Przedrostek = przedrostek Then
                cboPoleCentLadunek.SelectedItem = cboPoleCentLadunek.Items(i)
                LadunekCentJedn = j
                Exit Sub
            End If

        Next
    End Sub

    Private Sub lvCiala_SelectedIndexChanged() Handles lvCiala.SelectedIndexChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        zdarzenia = False

        If lvCiala.SelectedItems Is Nothing Then
            ZaznaczCialo(-1)
        Else
            If lvCiala.SelectedItems.Count = 0 Then
                ZaznaczCialo(-1)
            Else
                ZaznaczCialo(Integer.Parse(lvCiala.SelectedItems(0).SubItems(3).Text))
            End If
        End If
        Maluj()

        zdarzenia = True
    End Sub

    Private Sub btnPoleCentUsun_Click() Handles btnPoleCentUsun.Click
        zapisany = False
        zdarzenia = False

        Ciala(zaznaczone_id) = Nothing
        For i As Integer = 0 To lvCiala.Items.Count - 1
            If lvCiala.Items(i).SubItems(3).Text = zaznaczone_id.ToString Then
                lvCiala.Items(i).Remove()
                Exit For
            End If
        Next
        ZaznaczCialo(-1)

        Maluj()
        zdarzenia = True
    End Sub

    Private Sub txtPoleCentX_TextChanged() Handles txtPoleCentX.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtPoleCentX, popr)
        If popr Then
            zaznaczone.X0 = d
            zaznaczone.X = CInt(d / Skala0)
            zaznaczone.Xm = d * SkalaJedn.Mnoznik
            Maluj()
        End If
    End Sub

    Private Sub txtPoleCentY_TextChanged() Handles txtPoleCentY.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtPoleCentY, popr)
        If popr Then
            zaznaczone.Y0 = d
            zaznaczone.Y = CInt(d / Skala0)
            zaznaczone.Ym = d * SkalaJedn.Mnoznik
            Maluj()
        End If
    End Sub

    Private Sub txtPoleCentMasa_TextChanged() Handles txtPoleCentMasa.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleCentMasa, popr)
        If popr Then
            zaznaczone.Masa0 = d
            zaznaczone.Masa = d * zaznaczone.MasaJedn.Mnoznik / 1000
            lvCiala.Items(zaznaczone.ListViewId).SubItems(1).Text = zaznaczone.Masa0.ToString & " " & zaznaczone.MasaJedn.Przedrostek
        End If
    End Sub

    Private Sub txtPoleCentLadunek_TextChanged() Handles txtPoleCentLadunek.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        Dim popr As Boolean = False
        Dim d As Double = SprPopr(txtPoleCentLadunek, popr)
        If popr Then
            zaznaczone.Ladunek0 = d
            zaznaczone.Ladunek = d * zaznaczone.LadunekJedn.Mnoznik
            lvCiala.Items(zaznaczone.ListViewId).SubItems(2).Text = zaznaczone.Ladunek0.ToString & " " & zaznaczone.LadunekJedn.Przedrostek
        End If
    End Sub

    Private Sub cboPoleCentMasa_SelectedIndexChanged() Handles cboPoleCentMasa.SelectedIndexChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        If cboPoleCentMasa.SelectedItem Is Nothing Then Exit Sub
        MasaCentJedn = DirectCast(cboPoleCentMasa.SelectedItem, Jednostka)
        If zaznaczone Is Nothing Then Exit Sub
        zaznaczone.MasaJedn = MasaCentJedn
        zaznaczone.Masa = zaznaczone.Masa0 * zaznaczone.MasaJedn.Mnoznik / 1000
        lvCiala.Items(zaznaczone.ListViewId).SubItems(1).Text = zaznaczone.Masa0.ToString & " " & zaznaczone.MasaJedn.Przedrostek
    End Sub

    Private Sub cboPoleCentLadunek_SelectedIndexChanged() Handles cboPoleCentLadunek.SelectedIndexChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False

        If cboPoleCentLadunek.SelectedItem Is Nothing Then Exit Sub
        LadunekCentJedn = DirectCast(cboPoleCentLadunek.SelectedItem, Jednostka)
        If zaznaczone Is Nothing Then Exit Sub
        zaznaczone.LadunekJedn = LadunekCentJedn
        zaznaczone.Ladunek = zaznaczone.Ladunek0 * zaznaczone.LadunekJedn.Mnoznik
        lvCiala.Items(zaznaczone.ListViewId).SubItems(2).Text = zaznaczone.Ladunek0.ToString & " " & zaznaczone.LadunekJedn.Przedrostek
    End Sub

    Friend Sub CzyscPoleCent()
        lblCialo.Text = ""
        txtPoleCentX.Text = ""
        txtPoleCentY.Text = ""
        txtPoleCentMasa.Text = ""
        txtPoleCentLadunek.Text = ""
        btnPoleCentUsun.Enabled = False
        txtPoleCentX.Enabled = False
        txtPoleCentY.Enabled = False
        txtPoleCentMasa.Enabled = False
        txtPoleCentLadunek.Enabled = False
        cboPoleCentMasa.Enabled = False
        cboPoleCentLadunek.Enabled = False
    End Sub

    Friend Sub ZaznaczCialo(ByVal id As Integer)
        If id = -1 Then

            zaznaczone = Nothing
            zaznaczone_id = -1

            CzyscPoleCent()

        Else

            zaznaczone = Ciala(id)
            zaznaczone_id = id

            btnPoleCentUsun.Enabled = True
            txtPoleCentX.Enabled = True
            txtPoleCentY.Enabled = True
            txtPoleCentMasa.Enabled = True
            txtPoleCentLadunek.Enabled = True
            cboPoleCentMasa.Enabled = True
            cboPoleCentLadunek.Enabled = True

            lblCialo.Text = "c" & (id + 1).ToString
            txtPoleCentX.Text = Ciala(id).X0.ToString
            txtPoleCentY.Text = Ciala(id).Y0.ToString
            txtPoleCentMasa.Text = Ciala(id).Masa0.ToString
            txtPoleCentLadunek.Text = Ciala(id).Ladunek0.ToString
            ZaznaczCentMasa(Ciala(id).MasaJedn.Przedrostek)
            ZaznaczCentLadunek(Ciala(id).LadunekJedn.Przedrostek)

            Dim s As String = id.ToString
            For i As Integer = 0 To lvCiala.Items.Count - 1

                If lvCiala.Items(i).SubItems(3).Text = s Then
                    lvCiala.Items(i).Focused = True
                    lvCiala.Items(i).Selected = True
                    Exit For
                End If

            Next

        End If

    End Sub

    '-------------------------------------------------------------Pole jednorodne-----------------------------------------------------
    'Pole jednorodne grawitacyjne

    'CheckBox
    Private Sub cbPoleJednGrawGora_CheckedChanged() Handles cbPoleJednGrawGora.CheckedChanged
        If cbPoleJednGrawGora.Checked Then txtPoleJednGrawGora.Enabled = True Else txtPoleJednGrawGora.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Gora = cbPoleJednGrawGora.Checked
    End Sub

    Private Sub cbPoleJednGrawDol_CheckedChanged() Handles cbPoleJednGrawDol.CheckedChanged
        If cbPoleJednGrawDol.Checked Then txtPoleJednGrawDol.Enabled = True Else txtPoleJednGrawDol.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dol = cbPoleJednGrawDol.Checked
    End Sub

    Private Sub cbPoleJednGrawLewo_CheckedChanged() Handles cbPoleJednGrawLewo.CheckedChanged
        If cbPoleJednGrawLewo.Checked Then txtPoleJednGrawLewo.Enabled = True Else txtPoleJednGrawLewo.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Lewo = cbPoleJednGrawLewo.Checked
    End Sub

    Private Sub cbPoleJednGrawPrawo_CheckedChanged() Handles cbPoleJednGrawPrawo.CheckedChanged
        If cbPoleJednGrawPrawo.Checked Then txtPoleJednGrawPrawo.Enabled = True Else txtPoleJednGrawPrawo.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Prawo = cbPoleJednGrawPrawo.Checked
    End Sub

    'Pola tekstowe
    Private Sub txtPoleJednGrawGora_TextChanged() Handles txtPoleJednGrawGora.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednGrawGora, popr)
        If popr Then GoraIle = d
    End Sub

    Private Sub txtPoleJednGrawDol_TextChanged() Handles txtPoleJednGrawDol.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednGrawDol, popr)
        If popr Then DolIle = d
    End Sub

    Private Sub txtPoleJednGrawLewo_TextChanged() Handles txtPoleJednGrawLewo.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednGrawLewo, popr)
        If popr Then LewoIle = d
    End Sub

    Private Sub txtPoleJednGrawPrawo_TextChanged() Handles txtPoleJednGrawPrawo.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednGrawPrawo, popr)
        If popr Then PrawoIle = d
    End Sub

    'Pole jednorodne elektrostatyczne - Góra/dół
    Private Sub cbPoleJednElGora_CheckedChanged() Handles cbPoleJednElGora.CheckedChanged
        If cbPoleJednElGora.Checked Then pnlPoleJednElGora.Enabled = True Else pnlPoleJednElGora.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        GoraDol = cbPoleJednElGora.Checked
    End Sub

    Private Sub rbPoleJednElGoraPlus_CheckedChanged() Handles rbPoleJednElGoraPlus.CheckedChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        If rbPoleJednElGoraPlus.Checked Then GoraPlus = True Else GoraPlus = False
    End Sub

    Private Sub txtPoleJednElGoraE_TextChanged() Handles txtPoleJednElGoraE.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednElGoraE, popr)
        If popr Then GoraDolIle = d
    End Sub

    'Lewo/prawo
    Private Sub cbPoleJednElLewo_CheckedChanged() Handles cbPoleJednElLewo.CheckedChanged
        If cbPoleJednElLewo.Checked Then pnlPoleJednElLewo.Enabled = True Else pnlPoleJednElLewo.Enabled = False
        If zdarzenia = False Then Exit Sub
        zapisany = False
        LewoPrawo = cbPoleJednElLewo.Checked
    End Sub

    Private Sub rbPoleJednElLewoPlus_CheckedChanged() Handles rbPoleJednElLewoPlus.CheckedChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        If rbPoleJednElLewoPlus.Checked Then LewoPlus = True Else LewoPlus = False
    End Sub

    Private Sub txtPoleJednElLewoE_TextChanged() Handles txtPoleJednElLewoE.TextChanged
        If zdarzenia = False Then Exit Sub
        zapisany = False
        Dim popr As Boolean = False
        Dim d As Double = SprDodatnieZero(txtPoleJednElLewoE, popr)
        If popr Then LewoPrawoIle = d
    End Sub

#End Region 'Zaznaczanie i edycja kontrolek

#Region "Edycja obrazu"

    Private Sub pctObraz_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pctObraz.MouseDown
        If Animacja Then Exit Sub
        zdarzenia = False
        Dim p As New Point(e.X, WysY - e.Y)

        Dim znaleziono As Boolean = False

        'Czy kliknięto wektor
        If panel_zazn = PanelZaznaczony.CialoProbne And (Math.Abs(CialoProbne.X + Wektor.X - p.X) < 10) And (Math.Abs(CialoProbne.Y + Wektor.Y - p.Y) < 10) Then
            co_zazn = CoZaznaczone.Wektor
            przesun_wsp = New Point(p.X, p.Y)
            Exit Sub
        End If

        'Czy kliknięto ciało próbne
        If Math.Sqrt((p.X - CialoProbne.X) ^ 2 + (p.Y - CialoProbne.Y) ^ 2) < promien Then
            co_zazn = CoZaznaczone.CialoProbne
            przesun_wsp = New Point(p.X, p.Y)
            If panel_zazn <> PanelZaznaczony.CialoProbne Then pnlCialoProbne_Click()
            Exit Sub
        End If

        'Czy kliknięto ciało
        If Ciala IsNot Nothing Then

            For i As Integer = Ciala.Length - 1 To 0 Step -1

                If Ciala(i) IsNot Nothing Then
                    If Math.Sqrt((p.X - Ciala(i).X) ^ 2 + (p.Y - Ciala(i).Y) ^ 2) < promien Then

                        'Zaznaczenie ciała
                        znaleziono = True
                        co_zazn = CoZaznaczone.Cialo
                        ZaznaczCialo(i)
                        przesun_wsp = New Point(p.X, p.Y)
                        If panel_zazn <> PanelZaznaczony.PoleCentralne Then pnlPoleCentralne_Click()
                        Maluj()
                        Exit Sub

                    End If
                End If

            Next

        End If

        'Dodaj ciało
        If Not znaleziono Then
            zapisany = False

            'Dodanie
            Dim ile As Integer
            If Ciala Is Nothing Then ile = 0 Else ile = Ciala.Length
            ReDim Preserve Ciala(ile)
            Ciala(ile) = New Cialo With {.X = p.X, .Y = p.Y, .Xm = p.X * Skala, .Ym = p.Y * Skala, .X0 = p.X * Skala0, .Y0 = p.Y * Skala0, .Masa = MasaCentJedn.Mnoznik / 1000, .Masa0 = 1, .MasaJedn = MasaCentJedn, .Ladunek = LadunekCentJedn.Mnoznik, .Ladunek0 = 1, .LadunekJedn = LadunekCentJedn}

            'Wyświetlenie
            Dim el(3) As String
            el(0) = "c" & (ile + 1).ToString
            el(1) = "1 " & Ciala(ile).MasaJedn.Przedrostek
            el(2) = "1 " & Ciala(ile).LadunekJedn.Przedrostek
            el(3) = ile.ToString
            Dim lvi As New ListViewItem(el)
            lvCiala.Items.Add(lvi)
            Ciala(ile).ListViewId = lvi.Index

            'Zaznaczenie
            ZaznaczCialo(ile)

            If panel_zazn <> PanelZaznaczony.PoleCentralne Then pnlPoleCentralne_Click()

            Maluj()
        End If

    End Sub

    Private Sub pctObraz_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pctObraz.MouseMove
        If co_zazn = CoZaznaczone.Nic Then Exit Sub
        zapisany = False
        Dim p As New Point(e.X, WysY - e.Y)

        Select Case co_zazn
            Case CoZaznaczone.CialoProbne
                CialoProbne.X += (p.X - przesun_wsp.X)
                CialoProbne.Y += (p.Y - przesun_wsp.Y)
                CialoProbne.X0 = CialoProbne.X * Skala0
                CialoProbne.Y0 = CialoProbne.Y * Skala0
                CialoProbne.Xm = CialoProbne.X * Skala
                CialoProbne.Ym = CialoProbne.Y * Skala
                przesun_wsp = New Point(p.X, p.Y)

                txtCialoPrX.Text = CialoProbne.X0.ToString
                txtCialoPrY.Text = CialoProbne.Y0.ToString

            Case CoZaznaczone.Cialo
                zaznaczone.X += (p.X - przesun_wsp.X)
                zaznaczone.Y += (p.Y - przesun_wsp.Y)
                zaznaczone.X0 = zaznaczone.X * Skala0
                zaznaczone.Y0 = zaznaczone.Y * Skala0
                zaznaczone.Xm = zaznaczone.X * Skala
                zaznaczone.Ym = zaznaczone.Y * Skala
                przesun_wsp = New Point(p.X, p.Y)

                txtPoleCentX.Text = zaznaczone.X0.ToString
                txtPoleCentY.Text = zaznaczone.Y0.ToString

            Case CoZaznaczone.Wektor
                Wektor.X += (p.X - przesun_wsp.X)
                Wektor.Y += (p.Y - przesun_wsp.Y)
                Wektor.X0 = Wektor.X * Skala0
                Wektor.Y0 = Wektor.Y * Skala0
                Wektor.Xm = Wektor.X * Skala
                Wektor.Ym = Wektor.Y * Skala
                przesun_wsp = New Point(p.X, p.Y)

                txtCialoPrv0x.Text = Wektor.X0.ToString
                txtCialoPrv0y.Text = Wektor.Y0.ToString
                txtCialoPrv0.Text = Math.Sqrt(Wektor.X0 ^ 2 + Wektor.Y0 ^ 2).ToString

        End Select

        Maluj()
    End Sub

    Private Sub pctObraz_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles pctObraz.MouseUp
        co_zazn = CoZaznaczone.Nic
        przesun_wsp = Nothing
        zdarzenia = True
    End Sub

#End Region 'Edycja obrazu    

#Region "Obsługa animacji"
    Private Delegate Sub PokazImgDS(ByVal img As Bitmap)
    Private PokazImgDim As New PokazImgDS(AddressOf PokazImgP)
    Private Sub PokazImgP(ByVal img As Bitmap)
        pctObraz.Image = img
    End Sub
    Friend Sub PokazImg(ByVal img As Bitmap)
        Me.Invoke(PokazImgDim, img)
    End Sub

    Private Delegate Sub PokazKlatkiDS(ByVal klatki As String)
    Private PokazKlatkiDim As New PokazKlatkiDS(AddressOf PokazKlatkiP)
    Private Sub PokazKlatkiP(ByVal klatki As String)
        lblFps.Text = klatki
    End Sub
    Friend Sub PokazKlatki(ByVal klatki As String)
        Me.Invoke(PokazKlatkiDim, klatki)
    End Sub

    Private Delegate Sub ZakonczDS()
    Private ZakonczDim As New ZakonczDS(AddressOf ZakonczP)
    Private Sub ZakonczP()
        Animacja = False
        btnStart.Text = "Start"
        btnStop.Enabled = False
        mnuMenu.Enabled = True
        MaximizeBox = True
        MaximumSize = New Size(0, 0)
        MinimumSize = New Size(0, 0)
        pnlSkala.Enabled = True
        pnlCialoProbne.Enabled = True
        pnlPoleCentralne.Enabled = True
        pnlPoleJednorodne.Enabled = True
        lblFps.Text = ""
        Maluj()

        If zamknij Then Me.Close()
    End Sub
    Friend Sub Zakoncz()
        Me.Invoke(ZakonczDim)
    End Sub
#End Region 'Obsługa animacji

    Friend Enum PanelZaznaczony
        Nic
        Skala
        CialoProbne
        PoleCentralne
        PoleJednorodne
    End Enum

    Friend Enum CoZaznaczone
        Nic
        Wektor
        CialoProbne
        Cialo
    End Enum

End Class