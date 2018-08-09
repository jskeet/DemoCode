#!/bin/bash

set -e

rm -rf pages
git clone https://github.com/jskeet/DemoCode.git -b gh-pages pages
rm -rf pages/Versioning
mkdir pages/Versioning

cp index-template.md pages/Versioning/index.md

for i in output/*; do
  pagename=$(basename $i)
  page=pages/Versioning/$pagename.md
  title=$(grep -e "^# " $i/notes.md | sed "s/# //g")
  echo -e "- [$title]($pagename.md)\n" >> pages/Versioning/index.md
  cp $i/notes.md $page
  echo -e "----Library code before:\n\`\`\`csharp" >> $page
  cat $i/Before.cs >> $page
  echo -e "\`\`\`\n----\nLibrary code after:\n\`\`\`csharp" >> $page
  cat $i/After.cs >> $page
  echo -e "\`\`\`\n----\nClient code:\n\`\`\`csharp" >> $page
  cat $i/Client.cs >> $page
  echo -e "\`\`\`\n----\nInitial results:\n\`\`\`csharp" >> $page
  cat $i/before-result.txt >> $page
  echo -e "\`\`\`\n----\nResults of running Client.exe before recompiling:\n\`\`\`csharp" >> $page
  cat $i/after-result-1.txt >> $page
  echo -e "\`\`\`\n----\nResults of running Client.exe after recompiling:\n\`\`\`csharp" >> $page
  cat $i/after-result-2.txt >> $page
  echo -e "\`\`\`\n----\n[Back to index](index.md)" >> $page
done
