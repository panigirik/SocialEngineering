#include <iostream>
#include <cstring>

using namespace std;

int main()
{
    const int BUFFER_SIZE = 10;
    char buffer[BUFFER_SIZE];

    cout << "=== Buffer Overflow Demonstration Program ===" << endl;
    cout << "Buffer size is " << BUFFER_SIZE << " characters." << endl;
    cout << "Enter a string: ";

    char input[100];  // large temporary input buffer
    cin.getline(input, 100);

    int inputLength = strlen(input);

    cout << "\nYou entered: " << input << endl;
    cout << "Input length: " << inputLength << endl;

    if (inputLength >= BUFFER_SIZE)
    {
        cout << "\nWARNING: Buffer overflow detected!" << endl;
        cout << "The input string is too long for the buffer." << endl;
        cout << "If copied without checking, it would overwrite memory!" << endl;
        cout << "Program terminated to prevent unsafe behavior." << endl;
    }
    else
    {
        strcpy(buffer, input);
        cout << "\nString safely copied into buffer." << endl;
        cout << "Buffer content: " << buffer << endl;
        cout << "Program finished successfully." << endl;
    }

    return 0;
}