Attribute VB_Name = "Calculator"
Option Explicit

Private Sub btnAdd_Click()
    Dim num1 As Double
    Dim num2 As Double
    Dim result As Double
    
    On Error GoTo ErrorHandler
    
    num1 = CDbl(txtNumber1.Text)
    num2 = CDbl(txtNumber2.Text)
    result = num1 + num2
    
    txtResult.Text = CStr(result)
    Exit Sub
    
ErrorHandler:
    MsgBox "Invalid input: " & Err.Description, vbCritical
End Sub

Private Sub btnSubtract_Click()
    Dim num1 As Double, num2 As Double
    num1 = Val(txtNumber1.Text)
    num2 = Val(txtNumber2.Text)
    txtResult.Text = CStr(num1 - num2)
End Sub

Private Sub Form_Load()
    txtNumber1.Text = "0"
    txtNumber2.Text = "0"
    txtResult.Text = "0"
End Sub
