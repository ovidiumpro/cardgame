#!/bin/bash

# The resulting JSON file
json_file="results.json"

# Initialize the JSON file
echo "{" > $json_file

# For each parameter passed to the script
for term in "$@"
do
    # Initialize the JSON array for the term
    echo "\"$term\": [" >> $json_file

    # Create a find command to find .cs files whose names contain the current term
    find ./ -type f -name "*$term*.cs" | while read -r file
    do
        # Escape special characters in file content
        content=$(cat "$file" | sed 's/"/\\"/g' | sed ':a;N;$!ba;s/\n/\\n/g')

        # Append the file information to the JSON array
        echo "{ \"filename\": \"$file\", \"content\": \"$content\" }," >> $json_file
    done

    # End the JSON array for the term
    echo "]," >> $json_file
done

# End the JSON object
echo "}" >> $json_file
