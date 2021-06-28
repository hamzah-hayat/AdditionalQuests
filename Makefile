all:
	make full

full:
	rm -f ../AdditionalQuests.zip
	mkdir src
	mkdir ./src/Quests
	cp ./AdditionalQuestsCode/AdditionalQuestsCode/AdditionalQuestsSubModule.cs ./src
	cp -r ./AdditionalQuestsCode/AdditionalQuestsCode/Quests ./src
	zip ../AdditionalQuests.zip -r ./bin SubModule.xml README.md ./src
	rm -rf ./src