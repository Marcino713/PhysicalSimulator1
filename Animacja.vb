Friend Module Animacja_Modul
    Friend PokazFps As Boolean = False

    'Rysunki wyświetlane
    Friend img1_b As BitmapBytesRGB24
    Friend img2_b As BitmapBytesRGB24

    'Tło (fragment rysunku pod ciałem próbnym)
    Friend img1_tlo As Bitmap
    Friend img1_tlo_b As BitmapBytesRGB24
    Friend img2_tlo As Bitmap
    Friend img2_tlo_b As BitmapBytesRGB24

    'Ciało próbne
    Friend cialo_rys As Bitmap
    Friend cialo_b As BitmapBytesRGB24

    Friend Animacja As Boolean = False
    Friend Pauza As Boolean = False
    Friend Koniec As Boolean = False

    Friend Sub Animuj()
        Const G As Double = 6.67 * 10 ^ (-11)
        Const K As Double = 8.99 * 10 ^ 9
        Dim vx As Double = Wektor.Xm
        Dim vy As Double = Wektor.Ym
        Dim ax As Double
        Dim ay As Double
        Dim fx As Double
        Dim fy As Double
        Dim masa As Double
        Dim ladunek As Double
        Dim czas As Double
        Dim poczatek As Date
        Dim x As Double = CialoProbne.Xm
        Dim y As Double = CialoProbne.Ym
        Dim p1 As New Point(CialoProbne.X, CialoProbne.Y)
        Dim p2 As New Point(CialoProbne.X, CialoProbne.Y)
        Dim p3 As New Point(CialoProbne.X, CialoProbne.Y)
        Dim r As Double
        Dim wsp As Double
        Dim img As BitmapBytesRGB24
        Dim img_tlo As BitmapBytesRGB24
        Dim gr As Graphics
        Dim pimg As Integer
        Dim ptlo As Integer
        Dim pcpr As Integer
        Dim kl_czas As Date = Now
        Dim kl_ile As Integer = 0
        Dim rct As Rectangle
        Dim ipocz As Integer
        Dim ikon As Integer
        Dim jpocz As Integer
        Dim jkon As Integer
        Dim m As Integer
        Dim n As Integer

        Select Case Rodzaj
            Case RodzajCiala.Proton
                masa = 1.67 * 10 ^ (-27)
                ladunek = 1.6 * 10 ^ (-19)
            Case RodzajCiala.Elektron
                masa = 9.11 * 10 ^ (-31)
                ladunek = -(1.6 * 10 ^ (-19))
            Case RodzajCiala.Inne
                masa = CialoProbne.Masa
                ladunek = CialoProbne.Ladunek
        End Select

        img1_b = New BitmapBytesRGB24(Okno.img1)
        img2_b = New BitmapBytesRGB24(Okno.img2)
        poczatek = Now

        'Animacja
        Try
            Do
                fx = 0
                fy = 0

                'Pole jednorodne grawitacyjne
                If Gora Then fy += GoraIle * masa
                If Dol Then fy -= DolIle * masa
                If Lewo Then fx -= LewoIle * masa
                If Prawo Then fx += PrawoIle * masa

                'Pole jednorodne elektrostatyczne
                If GoraDol Then
                    If GoraPlus Then
                        fy -= GoraDolIle * ladunek
                    Else
                        fy += GoraDolIle * ladunek
                    End If
                End If

                If LewoPrawo Then
                    If LewoPlus Then
                        fx += LewoPrawoIle * ladunek
                    Else
                        fx -= LewoPrawoIle * ladunek
                    End If
                End If

                'Pole centralne
                If Ciala IsNot Nothing Then
                    For i As Integer = 0 To Ciala.Length - 1
                        If Ciala(i) IsNot Nothing Then

                            r = Math.Sqrt((x - Ciala(i).Xm) ^ 2 + (y - Ciala(i).Ym) ^ 2)
                            If r = 0 Then Continue For

                            'Pole centralne grawitacyjne
                            wsp = (G * masa * Ciala(i).Masa) / (r ^ 3)
                            fx += wsp * (Ciala(i).Xm - x)
                            fy += wsp * (Ciala(i).Ym - y)

                            'Pole centralne elektrostatyczne
                            wsp = (K * ladunek * Ciala(i).Ladunek) / (r ^ 3)
                            fx -= wsp * (Ciala(i).Xm - x)
                            fy -= wsp * (Ciala(i).Ym - y)

                        End If
                    Next
                End If

                'Przyspieszenie
                ax = fx / masa
                ay = fy / masa

                'Droga
                czas = (Now - poczatek).TotalMilliseconds / 1000
                poczatek = Now
                x += vx * czas + 0.5 * ax * czas * czas
                y += vy * czas + 0.5 * ay * czas * czas

                'Prędkość
                vx += ax * czas
                vy += ay * czas

                'Wyczyszczenie miejsca po ciele
                If Okno.czy_img1 Then
                    img = img2_b
                    img_tlo = img2_tlo_b
                    gr = Okno.gr2
                Else
                    img = img1_b
                    img_tlo = img1_tlo_b
                    gr = Okno.gr1
                End If

                'Wycyszczenie miejsca po ciele
                If (p2.X > -Okno.promien) And (p2.X < (SzerX + Okno.promien)) And (p2.Y > -Okno.promien) And (p2.Y < (WysY + Okno.promien)) Then    'Czy ciało znajduje się na obszarze rysunku

                    rct = New Rectangle(p2.X - Okno.promien, WysY - p2.Y - Okno.promien, Okno.srednica, Okno.srednica)
                    If rct.X < 0 Then rct.X = 0
                    If (rct.X + Okno.srednica) > SzerX Then rct.X = SzerX - Okno.srednica
                    If rct.Y < 0 Then rct.Y = 0
                    If (rct.Y + Okno.srednica) > WysY Then rct.Y = WysY - Okno.srednica

                    img.LockBitmap(rct)

                    For i As Integer = 0 To Okno.srednica - 1
                        pimg = i * img.RowSizeBytes
                        ptlo = i * img_tlo.RowSizeBytes

                        For j As Integer = 0 To 3 * (Okno.srednica - 1)
                            img.ImageBytes(pimg) = img_tlo.ImageBytes(ptlo)
                            pimg += 1
                            ptlo += 1
                        Next

                    Next

                    img.UnlockBitmap()

                End If

                'Przesunięcie
                p1 = p2
                p2 = p3
                p3.X = CInt(x / Skala)
                p3.Y = CInt(y / Skala)

                'Tor lotu
                gr.DrawLine(Pens.Black, p1.X, WysY - p1.Y, p2.X, WysY - p2.Y)
                gr.DrawLine(Pens.Black, p2.X, WysY - p2.Y, p3.X, WysY - p3.Y)

                'Wyświetlenie ciała w nowej lokalizacji
                If (p3.X > -Okno.promien) And (p3.X < (SzerX + Okno.promien)) And (p3.Y > -Okno.promien) And (p3.Y < (WysY + Okno.promien)) Then    'Czy ciało znajduje się na obszarze rysunku

                    rct = New Rectangle(p3.X - Okno.promien, WysY - p3.Y - Okno.promien, Okno.srednica, Okno.srednica)
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

                    img.LockBitmap(rct)

                    'Skopiowanie tła
                    For i As Integer = 0 To Okno.srednica - 1
                        pimg = i * img.RowSizeBytes
                        ptlo = i * img_tlo.RowSizeBytes

                        For j As Integer = 0 To 3 * (Okno.srednica - 1)
                            img_tlo.ImageBytes(ptlo) = img.ImageBytes(pimg)
                            pimg += 1
                            ptlo += 1
                        Next

                    Next

                    'Skopiowanie ciała
                    For i As Integer = ipocz To ikon
                        pimg = m * img.RowSizeBytes + n
                        pcpr = i * cialo_b.RowSizeBytes + jpocz

                        For j As Integer = jpocz To jkon
                            If cialo_b.ImageBytes(pcpr) <> 255 Then img.ImageBytes(pimg) = cialo_b.ImageBytes(pcpr)
                            pcpr += 1
                            pimg += 1
                        Next

                        m += 1
                    Next

                    img.UnlockBitmap()

                End If

                If Okno.czy_img1 Then Okno.PokazImg(Okno.img2) Else Okno.PokazImg(Okno.img1)
                Okno.czy_img1 = Not Okno.czy_img1

                'Klatki
                If PokazFps Then
                    kl_ile += 1
                    If (Now - kl_czas).TotalMilliseconds > 1000 Then
                        Dim cz As Double = (Now - kl_czas).TotalMilliseconds / 1000
                        Okno.PokazKlatki(Math.Round(kl_ile / cz).ToString & " fps")
                        kl_czas = Now
                        kl_ile = 0
                    End If
                Else
                    kl_ile = 0
                End If

                'Pauza
                If Pauza Then
                    Do
                        Threading.Thread.Sleep(100)
                    Loop While Pauza And (Koniec = False)
                    kl_ile = 0
                    kl_czas = Now
                    poczatek = Now
                End If

                'Koniec
                If Koniec Then
                    Okno.Zakoncz()
                    Exit Sub
                End If
            Loop

        Catch
            MessageBox.Show("Siła wypadkowa działająca na ciało jest zbyt duża i komputer tego nie wytrzymał.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Okno.Zakoncz()
        End Try

    End Sub
End Module