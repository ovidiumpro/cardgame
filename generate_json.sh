output_file="unity_project_files.json"
echo '{"files":[' > $output_file
first_file=true

find ./Assets/Scripts/Visual -type f \( -iname \*.cs \) -not -name "$output_file" | while read -r file; do
  if [ "$first_file" = true ]; then
    first_file=false
  else
    echo ',' >> $output_file
  fi

  file_name=$(basename "$file")
  relative_file_path=$(realpath --relative-to="." "$file")
  content=$(sed -e 's/\\/\\\\/g' -e 's/"/\\"/g' -e 's/`/\\`/g' -e 's/\$/\\$/g' -e 's/^/"/' -e 's/$/"/' -e 's/$/ +/' "$file" | tr -d '\n')

  # Remove the trailing '+'
  content=${content%+}

  if [[ $file_name == *.cs ]]; then
    type="script"
  else
    type="unknown"
  fi

  echo "  {" >> $output_file
  echo "    \"fileName\": \"$file_name\"," >> $output_file
  echo "    \"relativeFilePath\": \"$relative_file_path\"," >> $output_file
  echo "    \"type\": \"$type\"," >> $output_file
  echo "    \"content\": $content" >> $output_file
  echo "  }" >> $output_file
done

echo ']}' >> $output_file