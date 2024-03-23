#pragma once

class PSString
{
public:
	PSString();
	~PSString();
	void concat(String str);
	void concat(char chr);
	
	void clear();
	
	char* c_str();
	
private:
	char* buffer;
	int size;
	int capacity;
	
	
};