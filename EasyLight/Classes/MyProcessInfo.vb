Imports System.Diagnostics

Public Class MyProcessInfo
#Region "Properties"
    Private Shared _CurrentProcess As Process
    Private Shared _PerfCounterCpu As PerformanceCounter
#End Region

#Region "Constructeur"
    Shared Sub New()
        _CurrentProcess = Process.GetCurrentProcess
        _PerfCounterCpu = New System.Diagnostics.PerformanceCounter("Process", "% Processor Time", _CurrentProcess.ProcessName, True)
    End Sub
#End Region

#Region "Fonctions"
    'Fonction qui retourne le nombre de bytes utilisé en mémoire
    Public Shared Function GetMemoryUsedSize() As Double
        Dim ret As Double = -1
        Try
            ret = _CurrentProcess.PrivateMemorySize64
        Catch ex As Exception
            ret = -1
        End Try
        Return ret
    End Function

    'Fonction qui retourne le pourcentage de cpu utilisé
    Public Shared Function GetCpuPercentage() As Double
        Dim ret As Double = -1
        Try
            ret = CInt(_PerfCounterCpu.NextValue()) / Environment.ProcessorCount
        Catch ex As Exception
            ret = -1
        End Try
        Return ret
    End Function
#End Region

End Class
