#include "Arduino.h"

#include <stdint.h>
#include <stddef.h>

#include "PSString.h"

#if defined(ESP_PLATFORM)
#define MALLOC ps_malloc
#else
#define MALLOC malloc
#endif

PSString::PSString()
{
#if defined(ESP_PLATFORM)
	if (psramInit()) {
		Serial.println("\nPSRAM is correctly initialized");
	}
	else {
		Serial.println("PSRAM not available");
	}
#endif
	
	buffer = (char *) MALLOC(1024 * sizeof(char)); 
	
	capacity = 1024;
	size = 0;
	buffer[0] = '\0';
}

PSString::~PSString()
{
	free(buffer);
}

void PSString::concat(String str)
{
	if (size + str.length() > capacity - 1)
	{
		capacity *= 2;
		char* newBuffer = (char *) MALLOC(capacity * sizeof(char)); 
		memcpy(newBuffer, buffer, size);
		free(buffer);
		buffer = newBuffer;
	}
	
	memcpy(buffer + size, str.c_str(), str.length());
	size += str.length();
	buffer[size] = '\0';
}
void PSString::concat(char chr)
{
	if (size + 1 > capacity - 1)
	{
		capacity *= 2;
		char* newBuffer = (char *) MALLOC(capacity * sizeof(char)); 
		memcpy(newBuffer, buffer, size);
		free(buffer);
		buffer = newBuffer;
	}
	
	buffer[size] = chr;
	size += 1;
	buffer[size] = '\0';
}
	
void PSString::clear()
{
	buffer[0] = '\0';
	size = 0;
}

char* PSString::c_str()
{
	return buffer;
}