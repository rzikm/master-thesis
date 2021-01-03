TEX_DIR=${CURDIR}
TEX_SRC=$(wildcard $(TEX_DIR)/*.tex)

IMG_DIR=img
IMG_SRC=$(wildcard $(IMG_DIR)/*.svg)
IMG_OUT=$(patsubst %.svg, %.pdf_tex, $(IMG_SRC))
IMG_PDF=$(patsubst %.svg, %.pdf, $(IMG_SRC))

PLOT_DIR=plots
PLOT_SRC=$(wildcard $(PLOT_DIR)/*.ps1)
PLOT_OUT=$(patsubst %.ps1, %.tex, $(PLOT_SRC))

all: thesis.pdf abstract.pdf abstractCZ.pdf

# LaTeX must be run multiple times to get references right
thesis.pdf: $(TEX_DIR)/thesis.tex $(TEX_SRC) bibliography.bib $(IMG_OUT) $(PLOT_OUT) abstract.txt
	pdflatex $<
	biber thesis
	makeglossaries thesis
	pdflatex $<
	pdflatex $<

abstract.pdf: abstract.tex abstract.xmpdata abstract.txt
	pdflatex $<

abstractCZ.pdf: abstractCZ.tex abstractCZ.xmpdata
	pdflatex $<

$(IMG_DIR)/%.pdf_tex: $(IMG_DIR)/%.svg
	inkscape --export-filename $(patsubst %.svg, %.pdf, $<) --export-latex $<

$(PLOT_DIR)/%.tex: $(PLOT_DIR)/%.ps1 utils/plot.ps1
	pwsh -NoLogo -NoProfile $<

images: $(IMG_OUT)

plots: $(PLOT_OUT)

clean:
	rm -f *.log *.dvi *.aux *.toc *.lof *.lot *.out *.bbl *.blg *.xmpi *.glg *.glo *.gls *.ist *.lol *.bcf
	rm -f $(IMG_PDF) $(IMG_OUT) $(PLOT_OUT)
	rm -f $(PLOT_DIR)/*.pdf $(PLOT_DIR)/*.eps
	rm -f thesis.pdf abstract.pdf
	rm -f thesis.xmpdata abstract.xmpdata
