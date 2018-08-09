#!/bin/bash

set -e

if [[ ! -d "output" ]]
then
  echo "No output directory. Run runall.sh?"
  exit
fi

rm -rf pages
git clone https://github.com/jskeet/DemoCode.git -q -b gh-pages pages
rm -rf pages/Versioning
mkdir pages/Versioning

cp index-template.md pages/Versioning/index.md

for i in output/*; do
  pagename=$(basename $i)
  page=pages/Versioning/$pagename.md
  title=$(grep -e "^# " $i/notes.md | sed "s/# //g")
  echo "Building $pagename"
  echo -e "- [$title]($pagename.md)\n" >> pages/Versioning/index.md
  echo -e "---\ntitle: $title\n---" >> $page
  cat $i/notes.md >> $page
  echo -e "\n----\nLibrary code before:\n\`\`\`csharp" >> $page
  cat $i/Before.cs >> $page
  echo -e "\`\`\`\n----\nLibrary code after:\n\`\`\`csharp" >> $page
  cat $i/After.cs >> $page
  echo -e "\`\`\`\n----\nClient code:\n\`\`\`csharp" >> $page
  cat $i/Client.cs >> $page
  echo -e "\`\`\`\n----\nInitial results:\n\`\`\`text" >> $page
  cat $i/before-result.txt >> $page
  echo -e "\`\`\`\n----\nResults of running Client.exe before recompiling:\n\`\`\`text" >> $page
  cat $i/after-result-1.txt >> $page
  echo -e "\`\`\`\n----\nResults of running Client.exe after recompiling:\n\`\`\`text" >> $page
  cat $i/after-result-2.txt >> $page
  echo -e "\`\`\`\n----\n[Back to index](index.md)" >> $page
done
