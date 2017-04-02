Imports System.IO

Friend Module App
    'Błędy
    Friend Const BLPUSTE As String = "Pole nie może być puste!"
    Friend Const BLNIEPOPRAWNE As String = "Wartość pola jest niepoprawna."
    Friend Const BLUJEMNEZERO As String = "Wartość pola musi być liczbą rzeczywistą dodatnią."
    Friend Const BLUJEMNE As String = "Wartość pola musi być liczbą nieujemną."

    'Dane [z 0 - wpisane w polu tekstowym, bez 0 - przemnożone przez jednostkę]
    'Skala
    Friend Skala0 As Double
    Friend SkalaJedn As New Jednostka("m", 1)
    Friend Skala As Double
    'Ciało próbne
    Friend CialoProbne As New Cialo With {.X = 20, .Xm = 20, .Y = 20, .Ym = 20, .X0 = 20, .Y0 = 20, .Masa = 1, .Masa0 = 1, .MasaJedn = New Jednostka("kg", 1000), .Ladunek = 0, .Ladunek0 = 0, .LadunekJedn = New Jednostka("C", 1)}
    Friend Wektor As New WektorKlasa With {.X = 14, .Xm = 14, .X0 = 14, .Y = 10, .Ym = 10, .Y0 = 10}
    Friend Rodzaj As RodzajCiala
    'Pole centralne
    Friend Ciala As Cialo()
    Friend MasaCentJedn As New Jednostka("kg", 1000)
    Friend LadunekCentJedn As New Jednostka("C", 1)
    'Pole jednorodne
    Friend Gora As Boolean
    Friend GoraIle As Double
    Friend Dol As Boolean
    Friend DolIle As Double
    Friend Lewo As Boolean
    Friend LewoIle As Double
    Friend Prawo As Boolean
    Friend PrawoIle As Double
    Friend GoraDol As Boolean
    Friend GoraPlus As Boolean
    Friend GoraDolIle As Double
    Friend LewoPrawo As Boolean
    Friend LewoPlus As Boolean
    Friend LewoPrawoIle As Double

    'Rysowanie
    Friend WysY As Integer
    Friend SzerX As Integer

    'Pliki
    Friend Plik As String = ""
    Friend Sciezka As String = ""
    Friend Okno As wndOkno

    'Czyści i ustawia na domyślne wartości zmiennych
    Friend Sub CzyscZmienne()
        Skala0 = 1
        SkalaJedn = New Jednostka("m", 1)
        Skala = 1

        CialoProbne = New Cialo With {.X = 20, .Xm = 20, .Y = 20, .Ym = 20, .X0 = 20, .Y0 = 20, .Masa = 1, .Masa0 = 1, .MasaJedn = New Jednostka("kg", 1000), .Ladunek = 0, .Ladunek0 = 0, .LadunekJedn = New Jednostka("C", 1)}
        Wektor = New WektorKlasa With {.X = 14, .Xm = 14, .X0 = 14, .Y = 10, .Ym = 10, .Y0 = 10}
        Rodzaj = RodzajCiala.Inne

        Ciala = Nothing
        MasaCentJedn = New Jednostka("kg", 1000)
        LadunekCentJedn = New Jednostka("C", 1)

        Gora = False
        GoraIle = 0
        Dol = False
        DolIle = 0
        Lewo = False
        LewoIle = 0
        Prawo = False
        PrawoIle = 0
        GoraDol = False
        GoraPlus = True
        GoraDolIle = 0
        LewoPrawo = False
        LewoPlus = True
        LewoPrawoIle = 0
    End Sub

    Friend Class Cialo
        Friend X As Integer = 0
        Friend Y As Integer = 0
        Friend Xm As Double = 0
        Friend Ym As Double = 0
        Friend X0 As Double = 0
        Friend Y0 As Double = 0
        Friend Masa As Double = 0
        Friend Masa0 As Double = 0
        Friend MasaJedn As New Jednostka("kg", 1000)
        Friend Ladunek As Double = 0
        Friend Ladunek0 As Double = 0
        Friend LadunekJedn As New Jednostka("C", 1)
        Friend ListViewId As Integer = -1
        Friend TekstX As Integer = 0
        Friend TekstY As Integer = 0
    End Class

    Friend Class Jednostka
        Friend Przedrostek As String
        Friend Mnoznik As Double

        Friend Sub New(ByVal nazwa_przedrostka As String, ByVal mnoznik_jednostki As Double)
            Me.Przedrostek = nazwa_przedrostka
            Me.Mnoznik = mnoznik_jednostki
        End Sub

        Public Overrides Function ToString() As String
            Return Me.Przedrostek
        End Function
    End Class

    Friend Class WektorKlasa
        Friend X As Integer = 0
        Friend Y As Integer = 0
        Friend Xm As Double = 0
        Friend Ym As Double = 0
        Friend X0 As Double = 0
        Friend Y0 As Double = 0
    End Class

    Friend Enum RodzajCiala
        Proton = 0
        Elektron = 1
        Inne = 2
    End Enum

    Friend Sub ZapiszPlik(ByVal ZapiszJako As Boolean)
        If Sciezka = "" Or ZapiszJako = True Then
            Okno.dlgZapisz.FileName = ""
            If Okno.dlgZapisz.ShowDialog = DialogResult.Cancel Then Exit Sub
            Sciezka = Okno.dlgZapisz.FileName
            Dim t As String() = Sciezka.Split(Path.PathSeparator)
            Plik = t(t.Length - 1)
        End If

        Try
            Dim fs As New FileStream(Sciezka, FileMode.Create, FileAccess.Write)
            Dim bw As New BinaryWriter(fs)

            'Skala
            bw.Write("Pole eg 1#")
            bw.Write(Skala0)
            bw.Write(Mid(SkalaJedn.Przedrostek, 1, SkalaJedn.Przedrostek.Length - 1))

            'Ciało próbne
            bw.Write(CialoProbne.X0)
            bw.Write(CialoProbne.Y0)
            bw.Write(CialoProbne.Masa0)
            bw.Write(Mid(CialoProbne.MasaJedn.Przedrostek, 1, CialoProbne.MasaJedn.Przedrostek.Length - 1))
            bw.Write(CialoProbne.Ladunek0)
            bw.Write(Mid(CialoProbne.LadunekJedn.Przedrostek, 1, CialoProbne.LadunekJedn.Przedrostek.Length - 1))
            bw.Write(Wektor.X0)
            bw.Write(Wektor.Y0)
            bw.Write(Rodzaj)

            'Pole centralne
            Dim ile As Integer = 0
            If Ciala IsNot Nothing Then
                For i As Integer = 0 To Ciala.Length - 1
                    If Ciala(i) IsNot Nothing Then ile += 1
                Next
            End If

            bw.Write(ile)
            If Ciala IsNot Nothing Then
                For i As Integer = 0 To Ciala.Length - 1
                    If Ciala(i) IsNot Nothing Then

                        With Ciala(i)
                            bw.Write(.X0)
                            bw.Write(.Y0)
                            bw.Write(.Masa0)
                            bw.Write(Mid(.MasaJedn.Przedrostek, 1, .MasaJedn.Przedrostek.Length - 1))
                            bw.Write(.Ladunek0)
                            bw.Write(Mid(.LadunekJedn.Przedrostek, 1, .LadunekJedn.Przedrostek.Length - 1))
                        End With

                    End If
                Next
            End If

            'Pole jednorodne
            bw.Write(Gora)
            bw.Write(GoraIle)
            bw.Write(Dol)
            bw.Write(DolIle)
            bw.Write(Lewo)
            bw.Write(LewoIle)
            bw.Write(Prawo)
            bw.Write(PrawoIle)
            bw.Write(GoraDol)
            bw.Write(GoraPlus)
            bw.Write(GoraDolIle)
            bw.Write(LewoPrawo)
            bw.Write(LewoPlus)
            bw.Write(LewoPrawoIle)

            bw.Close()
        Catch
            MessageBox.Show("Błąd przy próbie zapisu pliku", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function ZnajdzJednostke(ByVal Nazwa As String, ByVal przedr As String) As Jednostka
        For i As Integer = 0 To Okno.jednostki.Length - 1
            If Okno.jednostki(i).Przedrostek = Nazwa Then
                Return New Jednostka(Okno.jednostki(i).Przedrostek & przedr, Okno.jednostki(i).Mnoznik)
            End If
        Next
    End Function

    Friend Sub OtworzPlik()
        Try
            Dim fs As New FileStream(Sciezka, FileMode.Open, FileAccess.Read)
            Dim br As New BinaryReader(fs)

            If br.ReadString <> "Pole eg 1#" Then
                MessageBox.Show("Błąd przy próbie otwarcia pliku. Plik ma nieprawidłowy format.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error)
                br.Close()
                Exit Sub
            End If

            'Skala
            Skala0 = br.ReadDouble
            SkalaJedn = ZnajdzJednostke(br.ReadString, "m")
            Skala = Skala0 * SkalaJedn.Mnoznik

            'Ciało próbne
            CialoProbne.X0 = br.ReadDouble
            CialoProbne.Y0 = br.ReadDouble
            CialoProbne.Masa0 = br.ReadDouble
            CialoProbne.MasaJedn = ZnajdzJednostke(br.ReadString, "g")
            CialoProbne.Ladunek0 = br.ReadDouble
            CialoProbne.LadunekJedn = ZnajdzJednostke(br.ReadString, "C")
            Wektor.X0 = br.ReadDouble
            Wektor.Y0 = br.ReadDouble
            Rodzaj = CType(br.ReadInt32, RodzajCiala)

            CialoProbne.X = CInt(CialoProbne.X0 / Skala0)
            CialoProbne.Xm = CialoProbne.X0 * SkalaJedn.Mnoznik
            CialoProbne.Y = CInt(CialoProbne.Y0 / Skala0)
            CialoProbne.Ym = CialoProbne.Y0 * SkalaJedn.Mnoznik
            CialoProbne.Masa = CialoProbne.Masa0 * CialoProbne.MasaJedn.Mnoznik / 1000
            CialoProbne.Ladunek = CialoProbne.Ladunek0 * CialoProbne.LadunekJedn.Mnoznik
            Wektor.X = CInt(Wektor.X0 / Skala0)
            Wektor.Y = CInt(Wektor.Y0 / Skala0)
            Wektor.Xm = Wektor.X0 * SkalaJedn.Mnoznik
            Wektor.Ym = Wektor.Y0 * SkalaJedn.Mnoznik

            'Pole centralne
            Dim ile As Integer = br.ReadInt32
            Dim c As Cialo

            If ile = 0 Then
                Ciala = Nothing
            Else
                ReDim Ciala(ile - 1)

                For i As Integer = 0 To ile - 1
                    c = New Cialo

                    c.X0 = br.ReadDouble
                    c.Y0 = br.ReadDouble
                    c.Masa0 = br.ReadDouble
                    c.MasaJedn = ZnajdzJednostke(br.ReadString, "g")
                    c.Ladunek0 = br.ReadDouble
                    c.LadunekJedn = ZnajdzJednostke(br.ReadString, "C")

                    c.X = CInt(c.X0 / Skala0)
                    c.Xm = c.X0 * SkalaJedn.Mnoznik
                    c.Y = CInt(c.Y0 / Skala0)
                    c.Ym = c.Y0 * SkalaJedn.Mnoznik
                    c.Masa = c.Masa0 * c.MasaJedn.Mnoznik / 1000
                    c.Ladunek = c.Ladunek0 * c.LadunekJedn.Mnoznik

                    Ciala(i) = c
                Next

            End If

            'Pole jednorodne
            Gora = br.ReadBoolean
            GoraIle = br.ReadDouble
            Dol = br.ReadBoolean
            DolIle = br.ReadDouble
            Lewo = br.ReadBoolean
            LewoIle = br.ReadDouble
            Prawo = br.ReadBoolean
            PrawoIle = br.ReadDouble
            GoraDol = br.ReadBoolean
            GoraPlus = br.ReadBoolean
            GoraDolIle = br.ReadDouble
            LewoPrawo = br.ReadBoolean
            LewoPlus = br.ReadBoolean
            LewoPrawoIle = br.ReadDouble

            br.Close()
        Catch
            MessageBox.Show("Błąd przy próbie otwarcia pliku. Prawdopodobnie plik ma nieprawidłowy format lub nie istnieje.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

End Module