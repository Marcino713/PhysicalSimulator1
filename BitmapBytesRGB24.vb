'
'Rod Stephens - Visual Basic 2008 Warsztat programisty, Helion 2009
'
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Public Class BitmapBytesRGB24
    ' Publiczny dostêp do danych bajtowych obrazu.
    Public ImageBytes() As Byte
    Public RowSizeBytes As Integer
    Public Const PixelDataSize As Integer = 24

    ' Referencja do mapy bitowej.
    Private m_Bitmap As Bitmap

    ' Zapisuje referencjê do mapy bitowej.
    Public Sub New(ByVal bm As Bitmap)
        m_Bitmap = bm
    End Sub

    ' Dane mapy bitowej.
    Private m_BitmapData As BitmapData

    ' Blokuje dane mapy bitowej.
    Public Sub LockBitmap(ByVal obszar As Rectangle)
        ' Blokuje dane mapy bitowej.
        m_BitmapData = m_Bitmap.LockBits(obszar, _
            Imaging.ImageLockMode.ReadWrite, _
            Imaging.PixelFormat.Format24bppRgb)
        RowSizeBytes = m_BitmapData.Stride

        ' Przydziela pamiêæ mapie bitowej.
        Dim total_size As Integer = m_BitmapData.Stride * m_BitmapData.Height
        ReDim ImageBytes(total_size)

        ' Kopiuje dane do tablicy ImageBytes.
        Marshal.Copy(m_BitmapData.Scan0, ImageBytes, _
            0, total_size)
    End Sub

    ' Kopiuje dane z powrotem do obiektu Bitmap
    ' i zwalnia zasoby.
    Public Sub UnlockBitmap()
        ' Kopiuje dane z powrotem do mapy bitowej.
        Dim total_size As Integer = m_BitmapData.Stride * m_BitmapData.Height
        Marshal.Copy(ImageBytes, 0, _
            m_BitmapData.Scan0, total_size)

        ' Odblokowuje mapê bitow¹.
        m_Bitmap.UnlockBits(m_BitmapData)

        ' Zwalnia zasoby.
        ImageBytes = Nothing
        m_BitmapData = Nothing
    End Sub
End Class
